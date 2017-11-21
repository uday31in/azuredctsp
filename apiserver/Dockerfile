#docker build . -t uday31in/azuredctsp-apiserver

FROM prodataninja/ubuntu-python2.7

RUN set -x \
	&& apt-get update \
	&& pip install --upgrade pip \
	&& curl https://bootstrap.pypa.io/ez_setup.py -o - | python \
	&& pip install ez_setup  \
	&& wget https://github.com/google/or-tools/releases/download/v6.1/or-tools_python_examples_v6.1.4333.tar.gz \
	&& tar -zxf or-tools_python_examples_v6.1.4333.tar.gz \
	&& cd ortools_examples \
	&& python setup.py install \
	&& pip install --upgrade ortools
	
#RUN set -x \
#	&& python ortools_examples/examples/python/golomb8.py


ARG source=.
WORKDIR /app
EXPOSE 80
COPY ${source:-obj/Docker/publish} .

RUN set -x \
	&& pip install -r requirements.txt --user


ENTRYPOINT ["python", "app.py"]
