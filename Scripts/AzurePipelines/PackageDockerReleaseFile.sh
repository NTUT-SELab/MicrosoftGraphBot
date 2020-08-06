#!/bin/bash

mkdir docker
cp ./docker-compose.yml ./docker
cp -r ./Scripts/CreateContainerFolder/* ./docker
tar zcvf docker.tar.gz ./docker
