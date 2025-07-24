## How to run the application

- The WebAPI uses Rabbit MQ and Mongo. both of these tools are ran from Docker and there's no physical installation of them on machine
  - To run Mongo DB please run the following commands on your terminal:

        - docker pull mongo
        - docker run --name my-mongo -d -p 27017:27017 mongo
  -  --name my-mongo: assigns a name to the container for easier identification
  -  -d: Runs container in detached mode (in the background)
  -  -p 27017:27017: Maps port 27017 on the host to port 27017 inside the container

  
To ensure that data persists even after the container is stopped or removed, use docker volumes to map a directory on the host machine to the data directory within the container

        docker run --name my-mongo -d -p 27017:27017 -v /path/to/local/data:/data/db mongo
        
             