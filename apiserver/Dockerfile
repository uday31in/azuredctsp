#docker build . -t uday31in/azuredctsp-apiserver

FROM prodataninja/ubuntu-python2.7

RUN set -x \
	&& apt-get update \
	&& pip install --upgrade pip \
	&& curl https://bootstrap.pypa.io/ez_setup.py -o - | python \
	&& pip install ez_setup  

ARG source=.
WORKDIR /app
EXPOSE 80
COPY ${source:-obj/Docker/publish} .

RUN set -x \
	&& pip install -r requirements.txt --user


ENTRYPOINT ["python", "app.py"]
