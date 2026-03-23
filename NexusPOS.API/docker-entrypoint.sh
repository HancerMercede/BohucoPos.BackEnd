#!/bin/bash
set -e

echo "=== Running Database Migrations ==="

# Wait for database to be ready
until dotnet ef database update --connection "$ConnectionStrings__DefaultConnection" 2>/dev/null; do
    echo "Waiting for database..."
    sleep 2
done

echo "=== Migrations Complete ==="

# Start the API
exec dotnet NexusPOS.API.dll
