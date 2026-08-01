# Secure MCP Gateway & .NET Server Samples

Welcome to the Secure Model Context Protocol (MCP) Gateway! This repository demonstrates how to build, secure, and dynamically route multiple HTTP-based MCP servers using .NET.

## 🚀 Powered by `McpHttpServer`

Building an HTTP-based MCP server in .NET shouldn't be complicated. This entire architecture is built on top of the **[McpHttpServer](https://www.nuget.org/packages/McpHttpServer)** NuGet package. 

If you are looking to build robust, scalable, and seamless AI tools in .NET, `McpHttpServer` does the heavy lifting for you! It abstracts away the complex Streamable HTTP Protocol and JSON-RPC lifecycle, allowing you to focus purely on writing your custom logic. (It fully implements the official Streamable HTTP MCP standard, not just basic SSE!). 

**Why use `McpHttpServer`?**
- **Dead Simple Setup:** Spin up a fully compliant MCP server with just a few lines of code.
- **Attribute-Based Tooling:** Define your AI tools easily using `[McpTool]` and `[McpResource]`.
- **Lightweight & Fast:** Native .NET integration running natively on Kestrel.
- **Enterprise Ready:** Easily sits behind reverse proxies and API gateways (like the one in this project!).

---

## 🏗️ Architecture Overview

When deploying AI tools to production, you don't want to expose every individual MCP server directly to the internet or to untrusted clients. 

This solution introduces a **Centralized API Gateway** built with YARP (Yet Another Reverse Proxy). 
- **The Gateway (`McpGateway`)** acts as the single entry point. It enforces strict **JWT Authentication** and handles CORS for all incoming connections.
- **The Internal Servers (`Sample_HTTP_MCP` 1, 2, & 3)** run on private ports. They are shielded from direct access using a hidden secret (`X-Gateway-Secret`) and rely entirely on the Gateway for internet-facing security.
- **Dynamic Routing:** The Gateway uses a live `routes.csv` file. You can add new MCP servers to your internal network on the fly, and the Gateway will instantly route traffic to them without requiring a server restart!

## 🛠️ Getting Started (How to use this zip)

If you have downloaded this solution, follow these steps to get everything running locally:

### Prerequisites
- .NET 10 SDK
- An MCP Client (like the [MCP Inspector](https://github.com/modelcontextprotocol/inspector))

### 1. Boot up the Internal Servers
These servers contain the actual AI tools (powered by `McpHttpServer`). They run on ports `5176`, `5178`, and `5179`.
Open your terminal and run:
```bash
dotnet run --project "Sample _HTTP_MCP"
dotnet run --project Sample_HTTP_MCP_2
dotnet run --project Sample_HTTP_MCP_3
```

### 2. Boot up the Gateway
The Gateway runs on port `5177` and acts as the secure front door.
```bash
dotnet run --project McpGateway
```

### 3. Generate an Auth Token
Because the Gateway is secure, you need a JWT token to connect. We've included a helper endpoint for testing.
Navigate to `http://localhost:5177/generate-token` in your browser and copy the `token` string.

### 4. Connect via MCP Inspector
Run the MCP Inspector in a separate terminal: `npx @modelcontextprotocol/inspector`
- **Transport:** Streamable HTTP
- **URL:** `http://localhost:5177/mcp1` (Change to `/mcp2` or `/mcp3` to reach the other servers)
- **Custom Headers:** 
  - **Key:** `Authorization`
  - **Value:** `Bearer <YOUR_TOKEN>`

## 🔀 Dynamic Routing (routes.csv)
Inside the `McpGateway` folder, you will find a `routes.csv` file. This acts as a mock database for the Gateway.
```csv
mcp1,http://localhost:5176
mcp2,http://localhost:5178
mcp3,http://localhost:5179
```
The Gateway intercepts requests (e.g., `/mcp1`) and looks up the destination URL live. You can edit this file while the Gateway is running, and traffic will be redirected instantly!
