# 🐳 Docker Desktop Installation Guide for CloudCart

## Step 1: Download Docker Desktop

**Download Link**: https://www.docker.com/products/docker-desktop/

1. Click **"Download for Windows"**
2. The installer is about 500MB - 1GB
3. Save the file: `Docker Desktop Installer.exe`

---

## Step 2: Install Docker Desktop

### Installation Steps:

1. **Run** `Docker Desktop Installer.exe` as Administrator
2. **Accept** the license agreement
3. **Configuration options** (recommended settings):
   - ✅ **Use WSL 2 instead of Hyper-V** (recommended)
   - ✅ **Add shortcut to desktop**
4. Click **"OK"** to start installation
5. Wait 5-10 minutes for installation to complete
6. Click **"Close and restart"** when prompted

### ⚠️ System Requirements:

- Windows 10 64-bit: Pro, Enterprise, or Education (Build 19041 or higher)
- OR Windows 11 64-bit
- Hardware virtualization enabled in BIOS
- WSL 2 feature enabled (installer will enable it if needed)
- 4GB RAM minimum (8GB+ recommended)

---

## Step 3: Enable WSL 2 (Windows Subsystem for Linux)

If Docker asks you to enable WSL 2, run these commands in **PowerShell as Administrator**:

```powershell
# Enable WSL
dism.exe /online /enable-feature /featurename:Microsoft-Windows-Subsystem-Linux /all /norestart

# Enable Virtual Machine Platform
dism.exe /online /enable-feature /featurename:VirtualMachinePlatform /all /norestart

# Restart your computer
Restart-Computer

# After restart, set WSL 2 as default
wsl --set-default-version 2
```

### Install Ubuntu (optional, for WSL 2):
```powershell
# Install Ubuntu from Microsoft Store
wsl --install -d Ubuntu
```

---

## Step 4: Start Docker Desktop

1. **Launch** Docker Desktop from Start Menu or Desktop
2. **Accept** the Docker Subscription Service Agreement
3. **Wait** for Docker Engine to start (whale icon in system tray will stop animating)
4. You'll see "Docker Desktop is running" notification

### First Time Setup:
- Skip the tutorial (or complete it if you want)
- Docker is ready when the whale icon is steady (not animating)

---

## Step 5: Verify Docker Installation

Open a **new PowerShell terminal** and run:

```powershell
# Check Docker version
docker --version

# Check Docker Compose version
docker compose version

# Test Docker is working
docker run hello-world
```

You should see:
```
Docker version 24.x.x, build xxxxx
Docker Compose version v2.x.x

Hello from Docker!
This message shows that your installation appears to be working correctly.
```

---

## Step 6: Configure Docker Settings (Optional but Recommended)

### Open Docker Desktop Settings:
1. Right-click the Docker whale icon in system tray
2. Click **"Settings"**

### Recommended Settings:

#### **Resources → Advanced**
- **CPUs**: 4 (or half of your total)
- **Memory**: 4-8 GB
- **Swap**: 1 GB
- **Disk image size**: 60 GB

#### **Docker Engine** (for better performance)
Add this configuration:
```json
{
  "builder": {
    "gc": {
      "defaultKeepStorage": "20GB",
      "enabled": true
    }
  },
  "experimental": false,
  "features": {
    "buildkit": true
  }
}
```

#### **General**
- ✅ Start Docker Desktop when you log in
- ✅ Use WSL 2 based engine

---

## Step 7: Start CloudCart Microservices

Once Docker is installed and running, navigate to your project:

```powershell
# Navigate to project directory
cd "C:\Users\Montell Boks\Desktop\CloudCartMicroservices"

# Start all services with Docker Compose
docker compose up --build -d

# This will start:
# ✅ MongoDB
# ✅ RabbitMQ
# ✅ Redis
# ✅ API Gateway
# ✅ Product Catalog Service
# ✅ Shopping Cart Service
# ✅ Order Service
# ✅ Payment Service
# ✅ User Service
```

### Wait 2-3 minutes for all services to start, then check:

```powershell
# View running containers
docker compose ps

# View logs
docker compose logs -f

# Test the API
Invoke-RestMethod -Uri 'http://localhost:5001/health'
```

---

## Step 8: Access Your Services

Once everything is running:

### **APIs & UIs:**
- **Product API Swagger**: http://localhost:5001/swagger
- **Shopping Cart API**: http://localhost:5002/swagger
- **Order API**: http://localhost:5003/swagger
- **Payment API**: http://localhost:5004/swagger
- **User API**: http://localhost:5005/swagger
- **API Gateway**: http://localhost:5000

### **Infrastructure UIs:**
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)
- **MongoDB**: mongodb://localhost:27017

---

## 🔍 Troubleshooting

### Docker Desktop won't start
**Solution 1**: Enable Virtualization in BIOS
- Restart computer → Enter BIOS (F2, F10, or DEL during boot)
- Find "Virtualization Technology" or "Intel VT-x" or "AMD-V"
- Enable it → Save and exit

**Solution 2**: Update Windows
- Go to Settings → Update & Security → Windows Update
- Install all updates
- Restart computer

**Solution 3**: Reinstall WSL 2
```powershell
# Run as Administrator
wsl --update
wsl --set-default-version 2
```

### Port conflicts (port already in use)
```powershell
# Find process using port 5001
netstat -ano | findstr :5001

# Kill process (replace PID with actual process ID)
taskkill /PID <PID> /F
```

### Containers won't start
```powershell
# Stop all containers
docker compose down

# Remove all containers and volumes
docker compose down -v

# Rebuild and start
docker compose up --build
```

### Docker is slow
- Increase RAM allocation in Docker Desktop settings
- Move Docker data directory to SSD
- Enable WSL 2 backend
- Close unnecessary applications

---

## 📚 Useful Docker Commands

```powershell
# Start services (detached mode)
docker compose up -d

# Start services (with logs visible)
docker compose up

# Stop services
docker compose down

# Stop and remove volumes (clean slate)
docker compose down -v

# View running containers
docker compose ps
docker ps

# View logs
docker compose logs
docker compose logs -f product-catalog
docker compose logs --tail=100 mongodb

# Restart a service
docker compose restart product-catalog

# Rebuild a specific service
docker compose build product-catalog
docker compose up -d product-catalog

# Execute commands in container
docker exec -it cloudcart-mongodb mongosh
docker exec -it cloudcart-rabbitmq rabbitmqctl status

# View container stats
docker stats

# Clean up unused resources
docker system prune -a
docker volume prune
```

---

## ✅ Installation Checklist

- [ ] Download Docker Desktop installer
- [ ] Run installer as Administrator
- [ ] Enable WSL 2 if prompted
- [ ] Restart computer if required
- [ ] Launch Docker Desktop
- [ ] Wait for Docker Engine to start
- [ ] Verify installation with `docker --version`
- [ ] Test with `docker run hello-world`
- [ ] Configure Docker resources (4GB RAM minimum)
- [ ] Navigate to CloudCart project directory
- [ ] Run `docker compose up --build -d`
- [ ] Wait 2-3 minutes for services to start
- [ ] Test API: http://localhost:5001/swagger
- [ ] Test RabbitMQ UI: http://localhost:15672
- [ ] Run test script: `.\test-api.ps1`

---

## 🎉 After Installation

Once Docker is running, you can:

1. **Start all services**: `docker compose up -d`
2. **Test the API**: Open http://localhost:5001/swagger
3. **View RabbitMQ events**: http://localhost:15672
4. **Check MongoDB data**: `docker exec -it cloudcart-mongodb mongosh`
5. **Run the test script**: `.\test-api.ps1`

---

## 🚀 Next Steps

After Docker is installed and running:

1. **Test all endpoints** with Swagger UI
2. **Create products** via API
3. **Watch RabbitMQ** for events
4. **Query MongoDB** for data
5. **Load test** with multiple requests
6. **Check logs** for any issues

---

## 💡 Alternative: Docker without Desktop

If Docker Desktop doesn't work, you can install:

### Option 1: Standalone MongoDB
```powershell
# Download from: https://www.mongodb.com/try/download/community
# Or use Chocolatey:
choco install mongodb
```

### Option 2: Standalone RabbitMQ
```powershell
# Download from: https://www.rabbitmq.com/download.html
# Or use Chocolatey:
choco install rabbitmq
```

### Option 3: Use Docker on WSL 2
```powershell
# Install Docker in WSL 2 Ubuntu
wsl
sudo apt update
sudo apt install docker.io docker-compose
sudo service docker start
```

---

## 📞 Need Help?

If you encounter issues:
1. Check Docker Desktop logs: Click whale icon → "Troubleshoot" → "View logs"
2. Restart Docker Desktop
3. Restart your computer
4. Check Windows Event Viewer for errors
5. Visit Docker documentation: https://docs.docker.com/desktop/troubleshoot/overview/

---

**Happy Dockerizing! 🐳**
