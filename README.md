# QuantScopeApp - Advanced FIX Log Analysis & Monitoring

![QuantScopeApp](assets/demo.gif)

> **A simple yet powerful tool for monitoring, analyzing, and troubleshooting FIX protocol messages.**  
> QuantScopeApp helps financial institutions, brokers, and trading firms gain deeper insights into their FIX messages, ensuring smooth trade operations.

## Why QuantScopeApp?

In financial trading, **FIX (Financial Information eXchange) protocol** is the backbone of trade communication. However:

- **Trade failures and rejections** happen due to incorrect FIX messages
- **Debugging FIX logs is painful** due to the **complex raw format**
- **Real-time monitoring is missing**, leading to **delays in issue resolution**

### What QuantScopeApp Solves

✔ **Visualizes FIX Messages** → No more manually scanning raw logs  
✔ **Simplifies Debugging** → Quickly identify rejected orders, errors, and trade issues  
✔ **Streamlined Log Analysis** → Search, filter, and categorize FIX messages with ease  
✔ **Lightweight & Fast** → Minimal setup required, just run and start analyzing  

## Features

🔹 **FIX Message Explorer** → View all FIX messages in a structured format  
🔹 **Real-Time Log Parsing** → Upload FIX log files and analyze instantly  
🔹 **Message Filtering & Search** → Find specific orders, executions, or errors quickly  
🔹 **Lightweight, No Heavy Infra** → Runs with just Docker, PostgreSQL, and .NET backend  
🔹 **Built for Brokers & Trading Firms** → Focused on real-world trading workflows  

## Tech Stack

🔹 **Frontend:** React (Minimal UI for message analysis)  
🔹 **Backend:** .NET (C#)  
🔹 **Database:** PostgreSQL  
🔹 **Logging & Monitoring:** Serilog (Future: Loki/Grafana integration)  
🔹 **Deployment:** Docker + GitHub Pages (for website)  

## Getting Started

### Run with Docker Compose

```sh
git clone https://github.com/yourusername/QuantScopeApp.git
cd QuantScopeApp
docker-compose up --build
```

The backend runs on http://localhost:9090  
The frontend runs on http://localhost:5173

## Future Roadmap

🔹 Real-time FIX Monitoring (Live FIX Session Analysis)  
🔹 Automated FIX Rule Validation  
🔹 Integration with Loki/Grafana for Advanced Log Monitoring  
🔹 Custom Alerts for Trade Failures  

---

🌟 **Star this repository** and share it with others.