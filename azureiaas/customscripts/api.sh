sudo apt-get update

curl -fsSL get.docker.com -o get-docker.sh
sh get-docker.sh

sudo git clone https://github.com/uday31in/azuredctsp.git /opt/azuredctsp

sudo apt-get install python-pip -y

sudo apt install virtualenv -y

sudo easy_install ortools

sudo virtualenv /opt/azuredctsp-run

sudo pip install -r /opt/azuredctsp/apiserver/requirements.txt



