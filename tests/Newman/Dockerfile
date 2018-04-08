FROM postman/newman_alpine33
COPY ./*.json /newman-tests/
WORKDIR /newman-tests
ENTRYPOINT ["npm","run","test"]