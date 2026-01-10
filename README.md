1. GitHub-Link
https://github.com/Koalabot95/MRP.git

2. Projektbeschreibung
Dieses Projekt implementiert einen HTTP-REST-Server in C#. 
Der Server unterstützt vollständige CRUD-Operationen für Benutzer 
(Registrierung, Login mit Bearer-Token-Authentifizierung, Aktualisieren 
und Löschen von Benutzerdaten) sowie für Media-Einträge.

Media-Einträge können bewertet (Ratings), kommentiert und als Favoriten markiert werden.
Die Authentifizierung erfolgt über Bearer Tokens, wobei Sessions und Tokens
serverseitig im Arbeitsspeicher (RAM) verwaltet werden. 

Je nachdem ob der Postgres-Container aktiv ist, werden User/Media Einträge 
lokal im RAM verwaltet oder in Postgres.

3. Get started
- Öffne Visual Studio
- bash 
git clone <https://github.com/Koalabot95/MRP.git>
cd MRP-Server
dotnet run
- Öffne Postman, benutze die Collection und lege User und Media-Einträge an 


