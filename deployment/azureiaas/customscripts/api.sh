sudo apt-get update

curl -fsSL get.docker.com -o get-docker.sh
sh get-docker.sh

sudo git clone https://github.com/uday31in/azuredctsp.git /opt/azuredctsp

sudo apt-get install python-pip -y
sudo apt install virtualenv -y
sudo easy_install ortools
sudo virtualenv /opt/azuredctsp-run
sudo pip install -r /opt/azuredctsp/apiserver/requirements.txt

sudo echo '
[Unit]
Description=apiservice

[Service]
Environment=SERVER_PORT=80
Environment=SERVER_HOST=0.0.0.0
ExecStart=/usr/bin/python2.7 /opt/azuredctsp/apiserver/app.py
WorkingDirectory=/opt/azuredctsp/apiserver

[Install]
WantedBy=multi-user.target

' > /etc/systemd/system/api.service

sudo systemctl enable api.service
sudo systemctl start  api.service
