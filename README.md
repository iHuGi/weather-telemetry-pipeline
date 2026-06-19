# C# Weather Telemetry Async Client

A lightweight, high-throughput asynchronous weather ingestion pipeline built in **C# (.NET)**. The system demonstrates modern **Task-based concurrency (async/await)**, parallel HTTP fan-out, and structured DTO-based data mapping for external API telemetry consumption.

Unlike traditional sequential API clients, this implementation leverages **Task.WhenAll orchestration** to execute concurrent HTTP requests, reducing total latency to the slowest network-bound operation.

---

## 1. Architectural Stack

* **Language:** C# (.NET 8+)
* **Concurrency Model:** Task Parallel Library (TPL)
* **HTTP Client:** `HttpClient` (persistent instance per service)
* **Data Parsing:** `Newtonsoft.Json (JObject)`
* **Configuration Management:** `DotNetEnv`
* **Runtime:** Cross-platform (.NET runtime on Linux/Windows)

---

## 2. Core Design Principles

### ⚙️ Asynchronous Fan-Out Model

The system issues multiple independent HTTP requests concurrently using a **fan-out/fan-in pattern**, where all city weather queries are executed in parallel and resolved via a single synchronization point.

### 🧠 DTO-Centric Data Mapping

All external API responses are mapped into a strongly structured `WeatherData` object, ensuring a clear separation between transport-layer JSON and application-level state.

### 🌐 Stateless Service Layer

`WeatherClient` operates as a stateless API wrapper, responsible solely for:

* Request construction
* HTTP execution
* Response deserialization

---

## 3. System Behavior

1. Load API key from `.env` using `DotNetEnv`
2. Instantiate a single `WeatherClient` (dependency-injected service layer)
3. Dispatch multiple HTTP requests in parallel using `Task.WhenAll`
4. Aggregate results into a single memory-resident array
5. Iterate over results and render telemetry output to console

---

## 4. Concurrency Model

The system uses a **bounded fan-out execution pattern**:

```text
Cities → Task List → Parallel HTTP Execution → Task.WhenAll → Result Aggregation
```

### Key Characteristics:

* Non-blocking I/O
* Single synchronization barrier
* Thread pool managed execution (no manual thread creation)
* Optimized for network-bound workloads

---

## 5. API Integration Layer

* **Provider:** OpenWeather API
* **Protocol:** HTTPS REST
* **Units:** Metric system (`units=metric`)
* **Endpoint Pattern:**

```
https://api.openweathermap.org/data/2.5/weather?q={city}&appid={API_KEY}
```

---

## 6. Build & Execution

### Prerequisites

* .NET 8 SDK
* Valid OpenWeather API key

---

### Configuration

Create a `.env` file in the root directory:

```bash
WEATHER_API=your_api_key_here
```

---

### Run

```bash
dotnet run
```

---

## 7. Output Example

```text
>> Lisbon <<
 Temp : 28.4 C
 Cond : Clear
-----------------------------------
>> Tokyo <<
 Temp : 21.9 C
 Cond : Clouds
-----------------------------------
```

---

## 8. Engineering Notes

* `Task.WhenAll` acts as a **synchronization barrier**, not a sequential loop
* `HttpClient` is designed for reuse; instantiation per request is intentionally avoided
* JSON parsing via `JObject` provides flexibility at the cost of performance overhead
* Execution time is strictly bounded by the slowest external API call (network latency dominant)

---

## 9. Repository Hygiene

* `.env` files are excluded from version control
* API keys are never hardcoded into source
* Stateless service design allows safe horizontal scaling of client instances
* No persistent storage layer (pure in-memory execution model)

---

## 10. System Classification

> This project is classified as a **network-bound asynchronous telemetry client**, designed to simulate real-world distributed data ingestion patterns using managed runtime concurrency primitives.

---