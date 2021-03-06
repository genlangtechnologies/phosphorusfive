#!/bin/bash

clear

# First giving user some information about what this script actually does.

echo "================================================================================"
echo "Automatic installation script for Phosphorus Five"
echo "Please let it finish, without interruptions, which might take some time."
echo ""
echo "The software is distributed in the hope that it will be useful,"
echo "but WITHOUT ANY WARRANTY; without even the implied warranty of"
echo "MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the"
echo "GNU General Public License for more details."
echo "================================================================================"
echo ""

# Download P5, and showing SHA1 to user, asking if he wants to proceed.
wget https://github.com/polterguy/phosphorusfive/releases/download/v4.9/binaries.zip
sha1sum binaries.zip

# Then asking user to confirm installation.
read -p "SHA1 of downloaded P5 zip file can be found above, continue? [y/n] " yn
if [[ ! $yn =~ ^[Yy]$ ]]; then
  exit
fi

# Installing MySQL server.
# Notice, by default MySQL is setup without networking, hence unless user explicitly opens it
# up later, this should be perfectly safe.
debconf-set-selections <<< 'mysql-server mysql-server/root_password password SomeRandomPassword'
debconf-set-selections <<< 'mysql-server mysql-server/root_password_again password SomeRandomPassword'
apt-get --assume-yes install apache2 mysql-server libapache2-mod-mono unzip gnupg2

# Removing any old files.
# Notice, we don't remove "/common" and "/users" here.
# This allows for a nice upgrading process (hopefully) without loosing old data in your system.
rm /var/www/html/index.html
rm /var/www/html/Default.aspx
rm /var/www/html/Global.asax
rm /var/www/html/README.md
rm /var/www/html/startup.hl
rm -r -f /var/www/html/bin
rm -r -f /var/www/html/desktop
rm -r -f /var/www/html/modules

# Creating a temporary folder to hold output.
mkdir p5

# Unzipping P5, in addition to moving it into main www/html folder.
unzip binaries.zip -d p5
cp -R p5/* /var/www/html

# Removing both zip file, and temp folder created during above process.
rm -f binaries.zip
rm -f -r p5

# Editing web.config file, making sure we get the password correctly.
sed -i 's/User Id=root;/User Id=root;password=SomeRandomPassword;/g' /var/www/html/web.config

# Giving ownership (recursively) to Apache user for entire folder.
# Necessary since P5 will create and modify its own file structure.
chown -R www-data:www-data /var/www

# Configuring mod_mono
echo "MonoAutoApplication enabled
AddType application/x-asp-net .aspx
AddType application/x-asp-net .asmx
AddType application/x-asp-net .ashx
AddType application/x-asp-net .asax
AddType application/x-asp-net .ascx
AddType application/x-asp-net .soap
AddType application/x-asp-net .rem
AddType application/x-asp-net .axd
AddType application/x-asp-net .cs
AddType application/x-asp-net .config
AddType application/x-asp-net .dll
DirectoryIndex index.aspx
DirectoryIndex Default.aspx
DirectoryIndex default.aspx
<FilesMatch \"^[^\.]+$\">
    ForceType application/x-asp-net
</FilesMatch>
<Files ~ \"\.hl\">
    Order allow,deny
    Deny from all
</Files>
<Location \"/users\">
    Order allow,deny
    Deny from all
</Location>
<Location \"/common\">
    Order allow,deny
    Deny from all
</Location>
" > /etc/apache2/mods-enabled/mod_mono_auto.conf

# Restarting Apache
service apache2 restart

# Informing user that his MySQL password can be found in web.config
echo "Your MySQL password can be found in the file '/var/www/html/web.config'"
