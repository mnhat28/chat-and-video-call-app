# 📡 TCP Chat Application (C# WinForms)

## 📝 Introduction
This is a simple chat application built using **TCP Socket** in C# with a **Client - Server** model.  
The server handles connections, user authentication, and broadcasts messages to all connected clients.

---

## ⚙️ Features

### 🔹 Server
- Listens on port `5555`
- Handles user authentication (hardcoded accounts)
- Receives and broadcasts messages to all clients
- Displays:
  - Connected clients
  - Chat messages

### 🔹 Client
- Connects to server via TCP
- User login system
- Sends and receives messages in real-time
- Displays chat in WinForms UI

---

## 👤 Sample Accounts
```
user1 / pass1
user2 / pass2
user3 / pass3
user4 / pass4
```

---

## 🚀 How to Run
1. Run the **Server** project  
2. Click the `Start` button to launch the server  
3. Run multiple **Client** instances  
4. Login and start chatting  

---

## 📌 Technologies Used
- C# WinForms  
- TCP Socket (`TcpListener`, `TcpClient`)  
- Multithreading (`Thread`)  
- Async/Await (client-side)  

---

## ⚠️ Notes
- Accounts are hardcoded (no database integration yet)  
- Thread handling can be improved (e.g., better synchronization)  
- File/image sending is not implemented yet  
