# Bohuco POS — Análisis de Lógica de Cuentas (Tab System)
> Documento de lógica de negocio. Define el comportamiento exacto del sistema de cuentas abiertas para restaurante y bar.

---

## 1. Concepto Central: La Cuenta (Tab)

Una **Cuenta** es el registro financiero activo de un cliente o grupo de clientes en una mesa o en el bar. Es diferente a una **Orden** (comanda puntual). La relación es:

```
Cuenta (Tab)
  └── Orden 1  (primer pedido)
        ├── Ítem: 2x Cerveza Artesanal
        └── Ítem: 2x Pollo a la Brasa
  └── Orden 2  (segundo pedido, misma cuenta)
        └── Ítem: 1x Postre
  └── Orden 3  (el cliente pide más cervezas)
        └── Ítem: 2x Cerveza Artesanal
```

El **total de la cuenta** es la suma acumulada de todas las órdenes asociadas a ella.

---

## 2. Identificación de una Cuenta

Una cuenta se identifica por **dos campos combinados**:

| Campo | Descripción | Ejemplo |
|---|---|---|
| `location` | Mesa o posición en el bar | "Mesa 1", "Barra 2" |
| `customerName` | Nombre del cliente (opcional en mesas, recomendado en barra) | "Juan", "Carlos" |

### Reglas:
- En **restaurante (mesas):** la cuenta puede abrirse solo con la mesa. El nombre es opcional pero recomendado.
- En **bar (barra):** el nombre del cliente es **obligatorio** porque múltiples clientes pueden estar en la misma barra.
- Si una mesa tiene varios clientes con cuentas separadas, cada cuenta lleva su propio nombre.

---

## 3. Flujo Completo — Restaurante (Mesa)

### Caso 1: Cliente en Mesa 1, cuenta única

```
1. Cliente llega y se sienta en Mesa 1
2. Mesero abre cuenta:
   → Location: "Mesa 1"
   → CustomerName: "Juan" (opcional)
   → Estado: OPEN

3. Cliente pide: 2 cervezas + 2 platos
   → Mesero crea Orden #1 ligada a la cuenta
   → Ítems van a cocina/barra vía comanda
   → Total cuenta: $XX

4. Cliente pide más cervezas (misma visita)
   → Mesero crea Orden #2 ligada a la MISMA cuenta
   → Total cuenta se acumula: $XX + $YY

5. Cliente pide la cuenta
   → Mesero imprime/muestra el resumen de la cuenta
   → Procesa el pago (efectivo / tarjeta)
   → Cuenta pasa a estado: CLOSED
   → Mesa queda libre
```

### Caso 2: Mesa con cuentas divididas (split)

```
Mesa 4 — 3 amigos que quieren pagar separado

Cuenta A → "Carlos"  →  Orden 1 (2 cervezas + 1 plato)
Cuenta B → "María"   →  Orden 1 (1 vino + 1 pasta)
Cuenta C → "Pedro"   →  Orden 1 (1 jugo + 1 pizza)

→ Cada uno paga su cuenta por separado
→ La mesa cierra cuando TODAS las cuentas están en CLOSED
```

---

## 4. Flujo Completo — Bar (Barra)

### Caso: Cliente en Barra, cuenta por nombre

```
1. Cliente llega a la barra
2. Mesero/bartender abre cuenta:
   → Location: "Barra 2"
   → CustomerName: "Luis"  ← OBLIGATORIO en barra
   → Estado: OPEN

3. Cliente pide 2 cervezas
   → Orden #1 → 2x Cerveza Artesanal → dest: Bar
   → Total cuenta: $13.00

4. Cliente pide 2 cervezas más (20 min después)
   → Orden #2 → 2x Cerveza Artesanal → dest: Bar
   → Total cuenta: $26.00

5. Cliente decide irse
   → Pide la cuenta → mesero cierra → pago → CLOSED
   ── ó ──
   → Mesero cierra manualmente si el cliente ya pagó en efectivo directo
```

---

## 5. Estados de una Cuenta

```
OPEN      → cuenta activa, puede recibir nuevas órdenes
PENDING   → cliente pidió la cuenta, esperando pago
CLOSED    → pagada y cerrada
CANCELLED → cancelada sin cobro (error, cortesía, etc.)
```

### Transiciones:
```
OPEN → PENDING   (cliente pide la cuenta)
OPEN → CLOSED    (mesero cierra directamente)
OPEN → CANCELLED (cancelación)
PENDING → CLOSED (pago procesado)
PENDING → OPEN   (cliente decide seguir — "dejame pedir algo más")
```

---

## 6. Modelo de Datos

### Tab (Cuenta)
```ts
interface Tab {
  id: string                    // "TAB-001"
  location: string              // "Mesa 1" | "Barra 2"
  customerName: string          // nombre del cliente
  waiterId: string              // mesero que abrió la cuenta
  status: TabStatus             // OPEN | PENDING | CLOSED | CANCELLED
  openedAt: string              // "12:30"
  closedAt?: string             // "14:15" — solo si está cerrada
  orders: Order[]               // todas las órdenes asociadas
  paymentMethod?: PaymentMethod // CASH | CARD | TRANSFER — al cerrar
  subtotal: number              // calculado: suma de todos los ítems
  tax: number                   // impuesto (configurable por negocio)
  total: number                 // subtotal + tax
  notes?: string                // observaciones del mesero
}

type TabStatus = 'OPEN' | 'PENDING' | 'CLOSED' | 'CANCELLED'
type PaymentMethod = 'CASH' | 'CARD' | 'TRANSFER'
```

### Relación Tab → Orders
```ts
// Una Tab tiene muchas Orders
// Una Order pertenece a una Tab

interface Order {
  id: string
  tabId: string          // ← FK a la Tab
  // ... resto igual que antes
}
```

---

## 7. Reglas de Negocio

### Apertura de cuenta:
- ✅ Una mesa puede tener **múltiples cuentas OPEN** simultáneamente (split)
- ✅ Una barra puede tener **múltiples cuentas OPEN** por distintos clientes
- ❌ No se puede abrir una cuenta en una mesa si no hay espacio disponible
- ⚠️ En barra, `customerName` es **obligatorio**

### Agregar órdenes:
- ✅ Solo se pueden agregar órdenes a cuentas en estado **OPEN**
- ❌ No se puede agregar a una cuenta PENDING, CLOSED o CANCELLED
- ✅ Cada nueva orden se acumula al total de la cuenta automáticamente

### Cierre de cuenta:
- ✅ El mesero puede cerrar directamente (OPEN → CLOSED)
- ✅ El cliente puede pedir la cuenta (OPEN → PENDING → CLOSED)
- ✅ Si el cliente pide más cosas después del PENDING → vuelve a OPEN
- ⚠️ La mesa solo queda **libre** cuando **todas** sus cuentas están CLOSED

### División de cuenta (split):
- ✅ El mesero puede mover ítems de una cuenta a otra dentro de la misma mesa
- ✅ Se pueden crear nuevas sub-cuentas en cualquier momento mientras la mesa está activa
- ❌ No se puede mover un ítem ya entregado (status: Delivered) a otra cuenta

---

## 8. Vista del Mesero — Cambios Necesarios en UI

### 8.1 Selección de Mesa — nuevo estado
```
Mesa libre      → verde   → click abre "Nueva Cuenta"
Mesa con cuenta → naranja → click muestra las cuentas activas
Mesa completa   → rojo    → todas las sillas ocupadas (futuro)
```

### 8.2 Modal "Abrir Cuenta"
```
Al hacer click en mesa libre:
  ┌─────────────────────────────┐
  │  Mesa 1 — Nueva Cuenta      │
  │                             │
  │  Nombre del cliente         │
  │  [ Juan                   ] │
  │                             │
  │  [Cancelar]  [Abrir Cuenta] │
  └─────────────────────────────┘

En barra: nombre obligatorio (validación)
En mesa: nombre opcional
```

### 8.3 Vista de Mesa con cuentas activas
```
Click en mesa ocupada muestra:
  ┌─────────────────────────────────────┐
  │  Mesa 4 — 3 cuentas activas         │
  │                                     │
  │  👤 Carlos    $45.00   [Ver cuenta] │
  │  👤 María     $28.50   [Ver cuenta] │
  │  👤 Pedro     $33.00   [Ver cuenta] │
  │                                     │
  │  [+ Nueva cuenta]                   │
  └─────────────────────────────────────┘
```

### 8.4 Vista de Cuenta individual
```
  ┌──────────────────────────────────────┐
  │  Mesa 4 — Carlos              OPEN   │
  │  Mesero: María  |  Abierta: 12:30    │
  ├──────────────────────────────────────┤
  │  ORDEN #1 — 12:30                    │
  │  ×2  Cerveza Artesanal      $13.00   │
  │  ×2  Pollo a la Brasa       $25.00   │
  │                                      │
  │  ORDEN #2 — 13:15                    │
  │  ×2  Cerveza Artesanal      $13.00   │
  ├──────────────────────────────────────┤
  │  Subtotal                   $51.00   │
  │  ITBIS (18%)                 $9.18   │
  │  TOTAL                      $60.18   │
  ├──────────────────────────────────────┤
  │  [+ Agregar Orden]                   │
  │  [Pedir Cuenta]  [Cerrar y Cobrar]   │
  └──────────────────────────────────────┘
```

---

## 9. Flujo en el Backend (.NET)

### Comandos nuevos necesarios:
```
OpenTabCommand          → abrir una nueva cuenta
AddOrderToTabCommand    → agregar orden a cuenta existente
RequestBillCommand      → cliente pide la cuenta (OPEN → PENDING)
CloseTabCommand         → cerrar y cobrar (→ CLOSED)
CancelTabCommand        → cancelar cuenta (→ CANCELLED)
MoveItemToTabCommand    → mover ítem entre cuentas (split)
```

### Queries nuevas:
```
GetActiveTabsByLocationQuery   → todas las cuentas abiertas de una mesa/barra
GetTabDetailsQuery             → detalle completo de una cuenta con sus órdenes
GetOpenTablesQuery             → resumen de mesas con cuentas activas
```

---

## 10. Resumen del Flujo Simplificado

```
RESTAURANTE:
Cliente llega → Mesero abre Tab (Mesa + Nombre opcional)
  → Cliente pide → Mesero crea Orden → va a cocina/barra
  → Cliente pide más → nueva Orden ligada a misma Tab
  → Cliente pide cuenta → Tab pasa a PENDING
  → Mesero cobra → Tab CLOSED → Mesa libre

BAR:
Cliente llega → Mesero abre Tab (Barra + Nombre obligatorio)
  → Cada ronda → nueva Orden ligada a la Tab del cliente
  → Cliente se va → Tab CLOSED (directo o via PENDING)
  → Barra sigue disponible para otros clientes con sus propias Tabs
```

---

*Versión 1.0 — Bohuco POS — Lógica de Cuentas — 2026*
