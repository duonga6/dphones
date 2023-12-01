FROM alpine
RUN apk add openssl 
RUN mkdir -p /etc/nginx/ssl && \
    openssl req -newkey rsa:2048 -nodes -keyout /etc/nginx/ssl/ca.key -x509 -days 365 -out /etc/nginx/ssl/ca.crt -batch