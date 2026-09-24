# EdgeMesh: Distributed Edge-AI Compute Orchestrator

[![C# .NET 8](https://img.shields.io/badge/Runtime-C%23%20.NET%208-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![llama.cpp](https://img.shields.io/badge/Local%20Inference-llama.cpp%20(GGUF)-blue?style=flat)](https://github.com/ggerganov/llama.cpp)
[![Security: TLS / RSA-256](https://img.shields.io/badge/Security-TLS%20%2F%20RSA--256-green?style=flat)](https://en.wikipedia.org/wiki/Transport_Layer_Security)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

A high-performance, cryptographically secure master-worker grid designed to orchestrate decentralized edge compute and local quantized LLM inference runtimes without relying on external cloud APIs.

---

## Architectural Topology

```
                  ┌─────────────────────────────────────┐
                  │       Central Mesh Controller       │
                  │   (Asynchronous TCPListener :8443)  │
                  └──────────────────┬──────────────────┘
                                     │
                    Encrypted TLS SslStream + RSA-256
                                     │
           ┌─────────────────────────┼─────────────────────────┐
           ▼                         ▼                         ▼
┌─────────────────────┐   ┌─────────────────────┐   ┌─────────────────────┐
│    Edge Node A      │   │    Edge Node B      │   │    Edge Node C      │
│                     │   │                     │   │                     │
│  [SecureAgent Host] │   │  [SecureAgent Host] │   │  [SecureAgent Host] │
│          │          │   │          │          │   │          │          │
│          ▼          │   │          ▼          │   │          ▼          │
│   [llama.cpp API]   │   │   [llama.cpp API]   │   │   [llama.cpp API]   │
│  (Mistral-7B Q4 GGUF│   │  (Mistral-7B Q4 GGUF│   │  (Mistral-7B Q4 GGUF│
└─────────────────────┘   └─────────────────────┘   └─────────────────────┘
```

---

## Key Capabilities

* **Decentralized Local Inference:** Nodes automatically provision lightweight `llama.cpp` runtimes and download 4-bit quantized GGUF models (`mistral-7b.Q4_K_M.gguf`), hosting isolated local inference APIs with zero external cloud billing.
* **Encrypted Network Mesh:** All control plane communications run over asynchronous .NET `SslStream` sockets, authenticated with RSA-signed tokens and AES session keys.
* **Real-Time Node Telemetry:** Central controller continuously monitors connected edge worker health, tracking CPU utilization, RAM consumption, and latency metrics with automated reconnection handling.
* **Dynamic Node Compilation:** Features integrated compilation facilities allowing worker stubs to be compiled on demand with runtime-tailored connection configurations.

---

## Repository Structure

```
├── SecureRemoteControlGUI.csproj   # .NET project configuration
├── ControllerManager.cs            # Central controller daemon & connection manager
├── SecureController.cs             # Asynchronous TCPListener & agent status tracker
├── SecureAgent.cs                  # Edge worker client & llama.cpp runtime bootstrapper
├── SecureAgentConnection.cs        # Authenticated socket session wrapper
├── SecureCompiler.cs               # Dynamic stub compilation engine
└── MainForm.cs                     # Operations telemetry monitoring GUI
```

---

## Building and Running

### Prerequisites
* .NET 8.0 SDK or higher
* Windows 10/11 or Windows Server (for GUI controller; agents compile cross-platform)

### Build
```bash
dotnet build -c Release
```

### Run Controller
```bash
dotnet run --project SecureRemoteControlGUI.csproj
```
The controller will initialize listening on port `8443` for TLS-authenticated edge node connections.
