# Deploying the Balsis Sheet Music Library

This guide will walk you through setting up the **Balsis Sheet Music Library** on your very own Linux server. For updating your app see [Updating.md](https://github.com/Jekabs-Leinis/BalsisSheetMusicLibrary/blob/master/Docs/Updating.md)

## 1. Prerequisites

* A fresh VPS running Ubuntu or Debian.
* Have the download link for the latest "self-contained release" from the Balsis GitHub Releases page ready.
* A web address (like `my-library.com`) that is already configured to point to your server's IP address.

## 2. Preparing Your Server

Connect to your server

```bash
ssh root@<YOUR_SERVER_IP>
```

Load info about latest software updates

```bash
sudo apt update
```

Install required tools

```bash
sudo apt install nginx ufw curl tar unzip wget -y
```

We need a dedicated folder to hold your app's files. We will also assign ownership of this folder to `www-data`, which is the default user account that Nginx uses to run websites securely.

Create the folder:

```bash
sudo mkdir -p /var/www/BalsisSheetMusicLibrary
```

Give ownership to the web user:

```bash
sudo chown -R www-data:www-data /var/www/BalsisSheetMusicLibrary
```

> [!IMPORTANT]
> If you later ever modify any files manually, for example replacing the app.db with your own preconfigured app.db, you will need to repeat this command.

## 3. Download the App

First, move into the folder we just created:

```bash
cd /var/www/BalsisSheetMusicLibrary
```

Download the release file to your server. Replace `<GITHUB_RELEASE_URL>` with the actual link to the `.zip` file from the [app's release page](https://github.com/Jekabs-Leinis/BalsisSheetMusicLibrary/releases). We run this as the web user (`www-data`) so permissions stay correct:

```bash
sudo -u www-data wget <GITHUB_RELEASE_URL> -O release.zip
```

Unzip the files:

```bash
sudo -u www-data unzip release.zip
```

Delete the downloaded zip file to keep things tidy:

```bash
sudo -u www-data rm release.zip
```

Give the server permission to run the app's main file:

```bash
sudo chmod +x /var/www/BalsisSheetMusicLibrary/BalsisSheetMusicLibrary.Server
```

*(Note: If you are an advanced user building the app on your own computer, you can securely copy your `./publish` folder directly to `/var/www/BalsisSheetMusicLibrary` using SCP instead of downloading it via wget).*

## 4. Set up .env configuration

Copy the example environment variable file to a new file named `.env`:

```bash
sudo -u www-data cp /var/www/BalsisSheetMusicLibrary/.env.example /var/www/BalsisSheetMusicLibrary/.env
```

Open the new `.env` file in the nano text editor:

```bash
sudo nano /var/www/BalsisSheetMusicLibrary/.env
```
> [!TIP]
> **How to save and exit in Nano:**
> 1. Press `Ctrl + O` (the letter O, not zero) to save.
> 2. Press `Enter` to confirm the file name.
> 3. Press `Ctrl + X` to exit.

Fill in the required values, especially the usernames and passwords. Adjust seeding flags as needed. Configure `LIB_SHEETS_FOLDER_PATH` to point to the folder where sheet music PDFs will be stored. This should be either a relative path from the server binary (e.g., `files/sheets`) or an absolute path (e.g., `/var/www/BalsisSheetMusicLibrary/files/sheets`).

If the folder specified in `LIB_SHEETS_FOLDER_PATH` does not exist, the app will attempt to create it automatically on startup.

It is recommended to turn the seeders on for the first run to create the initial admin and user accounts, then turn them off afterwards to prevent accidental password resets.

> [!NOTE]
> Without setting user passwords in the `.env` file, you will not be able to log in to the app.

## 5. Set up app service

If your server restarts, or if the app crashes, you want it to turn back on automatically. We use a built-in tool called **Systemd** to create a background service for your app.

Open a new file in a simple text editor called `nano`:

```bash
sudo nano /etc/systemd/system/BalsisSheetMusicLibrary.service
```

Copy the text below and paste it into the terminal (usually by right-clicking):

```ini
[Unit]
Description = Balsis Sheet Music Library

[Service]
WorkingDirectory = /var/www/BalsisSheetMusicLibrary
ExecStart = /var/www/BalsisSheetMusicLibrary/BalsisSheetMusicLibrary.Server
Restart = always
RestartSec = 10
SyslogIdentifier = BalsisSheetMusicLibrary
User = www-data
Environment="ASPNETCORE_ENVIRONMENT=Production"
Environment="DOTNET_PRINT_TELEMETRY_MESSAGE=false"

[Install]
WantedBy = multi-user.target
```

Load the new service configuration:
```bash
sudo systemctl daemon-reload
```

Enable the service to start on boot:
```bash
sudo systemctl enable BalsisSheetMusicLibrary.service
```

Start the service:
```bash
sudo systemctl start BalsisSheetMusicLibrary.service
```


## 6. Configure Nginx

Right now, your app is running internally on the server, but the outside world can't see it. We will configure Nginx to take requests from the internet and passing them to your app.

Create a new configuration file for your website:

```bash
sudo nano /etc/nginx/sites-available/BalsisSheetMusicLibrary
```

Paste the following text. **Important:** Change `example.com` to your actual domain name!

```nginx
server {
listen 80;
server_name example.com www.example.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }
}
```

Save and exit ( `Ctrl + O`, `Enter`, `Ctrl + X` ).

Create a symbolic link to enable the site:

```bash
sudo ln -s /etc/nginx/sites-available/BalsisSheetMusicLibrary /etc/nginx/sites-enabled/
```

Test Nginx configuration syntax:

```bash
sudo nginx -t
```

Restart Nginx:

```bash
sudo systemctl restart nginx
```

The library should be accessible at this point from your domain, though it will be using HTTP. If your application is not loading, check that your URL starts with `http://`. By default modern browsers will try to use HTTPS.

## 7. TLS / SSL Configuration
It is strongly recommended to set up HTTPS for your site.

Recommended option is to use Let's Encrypt, but other options exist.

Follow the [official Let's Encrypt guide](https://letsencrypt.org/getting-started/) to do this.

## 8. Open the Firewall

It is recommended to have a firewall enabled on your server for security.

Set up firewall rules for Nginx:
```bash
sudo ufw allow "Nginx Full"
sudo ufw allow "OpenSSH"
```

Verify that the rules are added:
```bash
sudo ufw show added
```

> [!WARNING]
> If you do not allow "OpenSSH" through the firewall, you may lock yourself out of your server and will potentially need to reinstall the operating system to regain access.

Enable the firewall:
```bash
sudo ufw enable
```
*(If it asks to confirm enabling the firewall, type `y` and press Enter).*

## 9. About Your Database

The application automatically uses a local database file named `app.db`. It lives inside your `/var/www/BalsisSheetMusicLibrary` folder.

If you want to back up your library data, simply download a copy of `app.db`.

> [!CAUTION]
> If you ever download a newer version of the app in the future, **do not** delete or overwrite your `app.db` file, or you will lose your saved music library!

## 10. Troubleshooting

If the website isn't loading, use these commands to find out why:

Check if the background service is running properly by typing:

```bash
sudo systemctl status BalsisSheetMusicLibrary.service
```
*(Look for a green "active (running)" message. Press `q` to exit this screen).*

Check the App Logs:
  ```bash
  sudo journalctl -u BalsisSheetMusicLibrary -f
  ```
*(Press `Ctrl + C` to exit)*

Check the Nginx Logs:
  ```bash
  sudo tail -f /var/log/nginx/error.log
  ```
*(Press `Ctrl + C` to exit)*
