# STADIO 1: Compilazione dell'applicazione Angular
FROM node:20-alpine AS build
WORKDIR /app

# Copia i file delle dipendenze (package.json e package-lock.json)
COPY package*.json ./

# Usa 'npm ci' invece di 'npm install' per un'installazione più pulita basata sul lockfile
RUN npm ci

# Copia tutto il resto del codice sorgente
COPY . .

# Compila l'applicazione.
# NOTA: se non hai ancora configurato l'ambiente "staging" in angular.json,
# puoi rimuovere "--configuration staging" per usare la build di produzione standard.
RUN npx ng build --configuration staging

# STADIO 2: Esecuzione tramite Server Web Nginx
FROM nginx:alpine

# Sostituisci "fe-roscoff" con il VERO nome del progetto: "bo-configurator"
COPY --from=build /app/dist/bo-configurator/browser /usr/share/nginx/html

# Sovrascrive la configurazione di default di Nginx per supportare il routing delle SPA...
RUN echo "server { \
    listen 80; \
    location / { \
        root /usr/share/nginx/html; \
        index index.html index.htm; \
        try_files \$uri \$uri/ /index.html; \
    } \
}" > /etc/nginx/conf.d/default.conf

# Espone la porta 80 pubblica
EXPOSE 80

# Avvia Nginx
CMD ["nginx", "-g", "daemon off;"]
