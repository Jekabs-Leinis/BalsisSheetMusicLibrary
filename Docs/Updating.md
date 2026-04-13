# Updating the Balsis Sheet Music Library

This guide will walk you through updating your **Balsis Sheet Music Library** to the latest version on your Linux server.

## 1. Prerequisites

* SSH access to your server where the app is currently running.
* Have the download link for the latest "self-contained release" from the Balsis GitHub Releases page ready.

## 2. Connect to Your Server

Connect to your server:

```bash
ssh root@<YOUR_SERVER_IP>
```

Navigate to the application folder:

```bash
cd /var/www/BalsisSheetMusicLibrary
```

## 3. Stop the Application

Before updating, stop the running service:

```bash
sudo systemctl stop BalsisSheetMusicLibrary.service
```

Verify the service has stopped:

```bash
sudo systemctl status BalsisSheetMusicLibrary.service
```

*(Look for "inactive (dead)". Press `q` to exit this screen).*

## 4. Backup Your Database

Create a timestamped backup of your database file:

```bash
cp /var/www/BalsisSheetMusicLibrary/app.db /var/www/BalsisSheetMusicLibrary/app.db_$(date +%F_%T).bak
```

This creates a backup file with the current date and time in its name, for example: `app.db_2026-04-13_14:30:45.bak`

> [!TIP]
> You can verify the backup was created by listing the files:
> ```bash
> ls -lh /var/www/BalsisSheetMusicLibrary/app.db*
> ```

## 5. Remove Old Application Files

Delete all old application files while preserving your `.env` configuration and database files:

```bash
find . -maxdepth 1 -type f ! -name '.env' ! -name 'app.db*' -delete
```

> [!WARNING]
> If you have any custom configuration or other files, perform a manual deletion

## 6. Download the New Version

Download the latest release file to your server. Replace `<GITHUB_RELEASE_URL>` with the actual link to the `.zip` file from the [app's release page](https://github.com/Jekabs-Leinis/BalsisSheetMusicLibrary/releases):

```bash
sudo -u www-data wget <GITHUB_RELEASE_URL> -O release.zip
```

Unzip the files (the `-n` flag prevents overwriting existing files like your database):

```bash
sudo -u www-data unzip -n release.zip
```

Delete the downloaded zip file:

```bash
sudo -u www-data rm release.zip
```

## 7. Set Correct Permissions

Ensure all files have the correct ownership:

```bash
sudo chown -R www-data:www-data /var/www/BalsisSheetMusicLibrary
```

Make the server executable:

```bash
sudo chmod +x /var/www/BalsisSheetMusicLibrary/BalsisSheetMusicLibrary.Server
```

## 8. Start the Application

Start the service again:

```bash
sudo systemctl start BalsisSheetMusicLibrary.service
```

Verify the service is running:

```bash
sudo systemctl status BalsisSheetMusicLibrary.service
```

*(Look for a green "active (running)" message. Press `q` to exit this screen).*

## 9. Verify the Update

Visit your website to confirm the update was successful. Check that:

* The site loads correctly
* You can log in
* Your sheet music library is intact
* All features work as expected

## 10. Troubleshooting

If something goes wrong after updating, you can restore your database backup:

```bash
sudo systemctl stop BalsisSheetMusicLibrary.service
cp /var/www/BalsisSheetMusicLibrary/app.db_<TIMESTAMP>.bak /var/www/BalsisSheetMusicLibrary/app.db
sudo systemctl start BalsisSheetMusicLibrary.service
```

*(Replace `<TIMESTAMP>` with the actual timestamp from your backup file name).*

Check the application logs if you encounter issues:

```bash
sudo journalctl -u BalsisSheetMusicLibrary -f
```

*(Press `Ctrl + C` to exit)*

Check the Nginx logs:

```bash
sudo tail -f /var/log/nginx/error.log
```

*(Press `Ctrl + C` to exit)*

## 11. Cleaning Up Old Backups

Over time, you may accumulate many database backup files. To list all backups:

```bash
ls -lh /var/www/BalsisSheetMusicLibrary/app.db*.bak
```

To remove old backups (be careful!), you can delete specific files:

```bash
sudo rm /var/www/BalsisSheetMusicLibrary/app.db_<TIMESTAMP>.bak
```

> [!CAUTION]
> Always keep at least one recent backup file in case you need to roll back!
