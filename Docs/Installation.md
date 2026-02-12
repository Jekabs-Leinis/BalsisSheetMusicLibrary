This document outlines the steps to deploy the self-contained Linux x64 release of the **Balsis Note Sheet Library** to a fresh Ubuntu or Debian VPS.
## 1. Preconditions

* A fresh VPS running Ubuntu or your favorite distro.
* Access to a self-contained release build. Download from [Releases]() or build your own using:

```bash
dotnet publish BalsisNoteSheetLibrary.Server --configuration Release --runtime linux-x64 --self-contained true -o ./publish
```
* A valid DNS entry pointing to your VPS IP address.

---

## 2. On-VPS Setup

### 2.1. Log in to your VPS via SSH and update the package list. Install Nginx and necessary utility tools.

Update repositories

```bash
sudo apt update
```

Install Nginx, Firewall, and utilities

```bash
sudo apt install nginx ufw curl tar unzip -y
```

### 2.2. Create the application directory and assign ownership to the `www-data` user.


Create directory

```bash
sudo mkdir -p /var/www/BalsisNoteSheetLibrary
```
Set ownership to www-data (Nginx default user)

```bash
sudo chown -R www-data:www-data /var/www/BalsisNoteSheetLibrary
```

---

## 3. Transfer and Install Files

You can deploy the files by downloading them directly from GitHub Releases or by uploading them from your local machine.

### Option A: Download from GitHub Releases (Recommended)

Navigate to the app directory:

```bash
cd /var/www/BalsisNoteSheetLibrary
```

Download the release (replace <GITHUB_RELEASE_URL> with the real URL). Run the command as the `www-data` user:

```bash
sudo -u www-data wget <GITHUB_RELEASE_URL> -O release.zip
```

Unzip the contents (run as `www-data`):

```bash
sudo -u www-data unzip release.zip
```

Cleanup (remove the downloaded zip file):

```bash
sudo -u www-data rm release.zip
```

### Option B: Upload via SCP

If building locally, upload the contents of your `./publish` folder.

Run this from your LOCAL machine:

```bash
scp -r ./publish/* user@<YOUR_VPS_IP>:/var/www/BalsisNoteSheetLibrary
```

### Final Permissions Check

Ensure the main executable has run permissions:

Make the binary executable:

```bash
sudo chmod +x /var/www/BalsisNoteSheetLibrary/BalsisNoteSheetLibrary.Server
```

Ensure `www-data` owns the files (if uploaded via SCP as root/user):

```bash
sudo chown -R www-data:www-data /var/www/BalsisNoteSheetLibrary
```

---

## 4. Systemd Service Configuration

Configure Systemd to manage the application process. This ensures the app restarts automatically if it crashes or the server reboots.

Create the service file:

```bash
sudo nano /etc/systemd/system/balsisnotesheetlibrary.service
```

Paste the following configuration verbatim into that file:

```ini
[Unit]
Description = Balsis Note Sheet Library

[Service]
WorkingDirectory = /var/www/BalsisNoteSheetLibrary
ExecStart = /var/www/BalsisNoteSheetLibrary/BalsisNoteSheetLibrary.Server
Restart = always
RestartSec = 10
SyslogIdentifier = BalsisNoteSheetLibrary
User = www-data
Environment="ASPNETCORE_ENVIRONMENT=Production"
Environment="DOTNET_PRINT_TELEMETRY_MESSAGE=false"

[Install]
WantedBy = multi-user.target
```

Save and exit the editor (e.g. `Ctrl+O`, `Enter`, `Ctrl+X` in nano).

Start the service by running the following commands one at a time.

Reload systemd to recognize the new file:

```bash
sudo systemctl daemon-reload
```

Enable the service to start on boot:

```bash
sudo systemctl enable balsisnotesheetlibrary.service
```

Start the service immediately:

```bash
sudo systemctl start balsisnotesheetlibrary.service
```

---

## 5. Nginx Reverse Proxy Configuration

Configure Nginx to proxy HTTP requests to the Kestrel server running on localhost:5000.

Create the site configuration file:

```bash
sudo nano /etc/nginx/sites-available/balsisnotesheetlibrary
```

Replace the `server_name` values with your actual domain (for example: `example.com www.example.com`).

Paste the following configuration into the file:

```nginx
server {
    listen 80;
    server_name example.com www.example.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }
}
```

Enable the site and restart Nginx by running these commands one at a time.

Create a symbolic link to enable the site:

```bash
sudo ln -s /etc/nginx/sites-available/balsisnotesheetlibrary /etc/nginx/sites-enabled/
```

Test Nginx configuration syntax:

```bash
sudo nginx -t
```

Restart Nginx:

```bash
sudo systemctl restart nginx
```

### TLS / SSL Configuration

Secure your deployment with HTTPS. **Follow the official guide here:** <LETS_ENCRYPT_TUTORIAL_URL>

---

## 6. Firewall Setup

Allow Nginx traffic through the firewall.

Allow HTTP/HTTPS traffic for Nginx:

```bash
sudo ufw allow 'Nginx Full'
```

Enable the firewall (if not already enabled):

```bash
sudo ufw enable
```

---

## 7. Database Persistence

The application uses a local SQLite database named `app.db` located in the application directory.

* **Persistence:** Ensure `app.db` is present in `/var/www/BalsisNoteSheetLibrary`.
* **Backups:** Regularly back up `/var/www/BalsisNoteSheetLibrary/app.db`.
* **Updates:** When deploying a new version, **DO NOT** overwrite `app.db` if you want to keep existing data.

---

## 8. Verification

1.  Check local connection (expected: HTTP 200 OK or similar):

```bash
curl -I http://localhost:5000
```

2.  Check public access: open `http://<YOUR_DOMAIN_OR_IP>` in a browser.

3.  Check service status:

```bash
sudo systemctl status balsisnotesheetlibrary.service
```

---

## 9. Troubleshooting

If the application is not accessible, check the following.

A. Application Logs — view the output from the .NET application:

```bash
sudo journalctl -u balsisnotesheetlibrary -f
```

B. Nginx Error Logs — check for proxy connection errors:

```bash
sudo tail -f /var/log/nginx/error.log
```

C. Port Conflicts — ensure port 5000 is being listened to by the application:

```bash
sudo ss -ltnp | grep 5000
```

D. Permission Issues — ensure the User in the systemd file matches the file owner. The following should show `www-data:www-data` as the owner/group:

```bash
ls -la /var/www/BalsisNoteSheetLibrary
```

---
