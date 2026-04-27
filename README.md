📱 QR Code Based Interactive AR Model Viewer

An Augmented Reality (AR) application built using Unity that allows users to scan QR codes and visualize corresponding 3D models in real-world space. The system also supports multi-device interaction using networking, enabling a shared AR experience.

🚀 Features
🔍 QR Code Scanning to trigger 3D models
🧊 Real-time AR Object Placement using plane detection
🤏 Touch Interaction (Rotate, Scale, Move)
📡 Multiplayer Support (Host-Client Architecture)
🔄 Real-time Sync of Object Transformations
📸 Screenshot Capture Functionality
🕶️ XR Device Testing (AR Glasses support)

🛠️ Technologies Used
Unity (2022.3 LTS recommended)
AR Foundation
ARCore (Android)
C# Scripting
Mirror Networking (for multiplayer)
Git & GitHub

📂 Project Structure
/Assets
  /Scripts        → C# scripts (QR, AR interaction, networking)
  /Prefabs        → 3D models and reusable components
  /Scenes         → Main AR scenes
  /UI             → Interface elements
/Packages
/ProjectSettings

⚙️ How It Works
User scans a QR code using the mobile device camera
QR data is processed and mapped to a specific 3D model
Model is loaded and placed in AR space using plane detection
Users can interact with the model using touch gestures

In multiplayer mode:
One device acts as Host
Other devices join as Clients
All interactions are synced in real-time

📡 Multiplayer Functionality
Host creates the AR session
Clients connect via local network
Object position, rotation, and scale are synchronized
Uses networked variables and commands for real-time updates

📱 Requirements
Hardware
Android device with ARCore support
(Optional) AR Glasses for XR testing
Software
Unity Hub
Android SDK
Git

▶️ Setup Instructions
Clone the repository
git clone https://github.com/shiv-2-s/AR-Shared_Display-Space-with-QR-Code-Access.git
Open project in Unity
Enable ARCore in XR Plugin Management
Build and run on Android device

🧪 Testing
Tested on multiple Android devices
Verified QR detection under different lighting conditions
Tested multiplayer sync (Host ↔ Client)
Real-world testing on surfaces like tables
XR testing using AR glasses

🐞 Known Issues
Minor delay in multiplayer synchronization under weak network
Tracking instability in low-light environments
QR detection accuracy depends on camera quality

📈 Future Improvements
Cloud Anchor support for persistent AR sessions
Cross-platform support (iOS)
UI/UX enhancements
Improved networking optimization
Model database expansion

📸 Screenshots

(Add screenshots here)

AR Model Placement
QR Scanning
Multiplayer Sync
👨‍💻 Author

Shivnand (Shiv)
BCA Student | Game Development Enthusiast | AR Developer


This project is for academic and learning purposes.
