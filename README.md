# tvshowapi

From root folder

- run `docker compose up -d` to run elastic search and kibana

- run data importer `cd src\TvShow.Importer &&  dotnet run` to fetch tv maze data

- run web api `cd src\TvShow.Api && dotnet run`

- navigate to http://localhost:8080/swagger
