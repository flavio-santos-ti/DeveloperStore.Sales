FROM postgres:latest

# Instalar pacotes necessários para o locale
RUN apt-get update && apt-get install -y locales

# Gerar e configurar o locale pt_BR.UTF-8
RUN echo "pt_BR.UTF-8 UTF-8" >> /etc/locale.gen && \
    locale-gen pt_BR.UTF-8 && \
    update-locale LANG=pt_BR.UTF-8

# Configurar variáveis de ambiente
ENV LANG=pt_BR.UTF-8
ENV LC_ALL=pt_BR.UTF-8

# Expor a porta do PostgreSQL
EXPOSE 5432
