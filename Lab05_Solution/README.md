# CS426L Lab 05 — Synchronization Mechanisms

Complete C#/.NET solution for the uploaded Lab 05 task sheet.

## Projects

- BufferMutex — Task 1
- BufferSemaphore — Task 2
- CondSignal — Task 3
- PhaseBarrier — Task 4
- LockFreeStack — Task 5 optional bonus

## Build

Open a terminal inside each project folder:

dotnet build -c Release

## Run

### Task 1
dotnet run --no-build -c Release

### Task 2
dotnet run --no-build -c Release -- 2 2 8

Other required configurations:
1 1 8
2 2 8
4 4 8
4 4 1
4 4 64
4 2 8

### Task 3
dotnet run --no-build -c Release -- cond
dotnet run --no-build -c Release -- cond 1 signal-first
dotnet run --no-build -c Release -- cond 3
dotnet run --no-build -c Release -- spin

### Task 4
dotnet run --no-build -c Release
dotnet run --no-build -c Release -- 8
dotnet run --no-build -c Release -- --no-barrier

### Task 5
dotnet run --no-build -c Release
dotnet run --no-build -c Release -- 8

Use your own machine's measured output for the lab tables.
