#!/bin/bash

# Script to test UI with Aspire orchestration
# This script starts Aspire, waits for services to be ready, runs UI tests, then cleans up

set -e  # Exit on any error

echo "🚀 Starting Aspire orchestration..."

# Start Aspire in the background
cd Billing/src/Billing.AppHost
dotnet run > aspire-test.log 2>&1 &
ASPIRE_PID=$!

echo "📱 Aspire started with PID: $ASPIRE_PID"

# Function to cleanup on exit
cleanup() {
    echo "🧹 Cleaning up..."
    kill $ASPIRE_PID 2>/dev/null || true
    wait $ASPIRE_PID 2>/dev/null || true
    echo "✅ Cleanup complete"
}

# Set trap to cleanup on script exit
trap cleanup EXIT

# Wait for Aspire dashboard to be ready
echo "⏳ Waiting for Aspire dashboard to be ready..."
timeout=120
counter=0
while ! curl -s http://localhost:18110 >/dev/null; do
    sleep 2
    counter=$((counter + 2))
    if [ $counter -gt $timeout ]; then
        echo "❌ Timeout waiting for Aspire dashboard"
        exit 1
    fi
done

# Wait for UI service to be ready
echo "⏳ Waiting for UI service to be ready..."
counter=0
while ! curl -s http://localhost:8105 >/dev/null; do
    sleep 2
    counter=$((counter + 2))
    if [ $counter -gt $timeout ]; then
        echo "⚠️  UI service not ready, but proceeding with tests..."
        break
    fi
done

echo "🧪 Running UI tests with Aspire..."
cd ../../../Billing/web/billing-ui

# Run the UI tests in Aspire mode
npm run test:ui-aspire

echo "✅ UI tests completed successfully!"