MRP Zwischenabgabe - Projektdokumentation

1. Ueberblick

Dieses Projekt implementiert einen HTTP-REST-Server in C#. Der Server unterstuetzt sowohl User CRUD-Operationen(User-Registrierung, Login mit Bearer-Token-Authentifizierung, User-Daten Updaten sowie User loeschen) als auch komplette CRUD-Operationen fuer Media-Eintraege. Sessions und Tokens werden akutell noch im RAM verwaltet.

2. Architektur

2.1 Listener & Dispatcher
Der HttpListener nimmt eingehende Requests entgegen und wandelt sie in HttpRestEventArgs um. Der zentrale Handler (Handler) ruft nun jeden Handler (UserHandler, MediaHandler, RatingHandler) auf und jeder bekommt das HttpRestEventArgs e Objekt. 

2.2 Handler-Konzept
Durch das Zerlegen des URL-Pfads ist ein einfaches Routing möglich. Jeder Handler prueft anhand des Request-Pfads, ob er zustaendig ist (z. B. e.Path.StartsWith("/users")).

2.3 Session- und Token-System
Nach erfolgreichem Login wird eine Session erzeugt, die Username, Token und Timestamp enthaelt. Tokens werden in einem Dictionary gespeichert. Lazy-Cleanup entfernt abgelaufene Sessions. Authentifizierte Endpoints nutzen GetSessionOrThrow zur Token-Pruefung. Die Token basieren auf 24 zufaelligen Zeichen. Geschuetzte Endpoints verlangen Authorization mittels Bearer token (Jeder der diesen Token hat, kann darauf zugreifen). Die Sessions verfallen ueber Cleanup oder wenn das Programm geschlossen wird. Der Session-Timestamp wird bei jeder erfolgreichen Verwendung aktualisiert.

3. Domain-Modelle

3.1 User
Enthaelt UserName, FullName, Email und PasswordHash (SHA256). UserName ist unveraenderbar nach der Registrierung.

3.2 Media
Enthaelt Id, Title, Description, Type, ReleaseYear, Genres, AgeRestriction, OwnerUserName.

3.3 Rating enthaelt Id, Media Id, Creator, Stars, Comment, Likes. (RatingHandler wurde hierzu noch nicht implementiert).

4. Repositories
CRUD-Operationen werden aktuell noch in Memory abgewickelt. UserRepository, MediaRepository und RatingRepository arbeiten mit Dictionaries als In-Memory-Speicher.

6. Integration Tests
Fuer die Zwischenabgabe wurde eine Postman Collection mit allen Endpoints bereitgestellt.

7. UML-Diagramme

7.1 Architektur

                         ┌──────────────────────────────┐
                         │        HttpRestServer        │
                         ├──────────────────────────────┤
                         │  + HttpListener              │
                         │  + Running : bool            │
                         │  + Run() : void              │
                         │  + Stop() : void             │
                         │  + Dispose() : void          │
                         └──────────────┬───────────────┘
                                        │
                                        |
                         ┌──────────────────────────────┐                      ┌──────────────────────────┐
                         │        static Handler        │                      │         IHandler         │
                         ├──────────────────────────────┤                      ├──────────────────────────┤
                         │ - _Handlers : List<IHandler> │----------------------| + Handle(e) : void       |
                         │ + HandleEvent(e) : void      │                      └──────────────────────────┘
                         │ + _GetHandlers()             |                                  |
                         | + Handle(e) : void           │                                  |
                         └──────────────┬───────────────┘                                  |
                                        │                                                  |
                                        |                                                  |
                                        |                                                  |
                                        |                                                  |
                                        |                                                  |
                                        |                                                  |
                                        |                                                  |
                 ──────────────────────────────────────────────────────────────────────────────────────────────────────────────
                |                                |                                         |                                   |
                |                                |                                         |                                   |
  ┌───────────────────────────────┐   ┌───────────────────────────────┐   ┌───────────────────────────────┐    ┌───────────────────────────────┐  
  │         MediaHandler          │   │        RatingHandler          │   │       VersionHandler          │    │          UserHandler          | 
  ├───────────────────────────────┤   ├───────────────────────────────┤   ├───────────────────────────────|    ├───────────────────────────────┤                                   
  | + Handle(e) : void            |   | + Handle(e) : void            |   | + Handle(e) : void            |    | + Handle(e) : void            |                                                         
  | + GetSessionOrThrow(e)        |   └───────────────────────────────┘   └───────────────────────────────┘    └───────────────────────────────┘
  └───────────────────────────────┘

7.2 Domänen
          ┌─────────────┐                                                                                                                    
          │IAtom        │                                                                         
          ├─────────────┤                                                                         
          │+ BeginEdit()│                                                                       
          │+ Save()     │                                                                          
          │+ Delete()   │                                                                           
          │+ Refresh()  │                                                                          
          └─────────────┘                                                                     
                  |                                               
                  |                                                                     
                  |                                                     ┌──────────────────────────────┐   ┌────────────────────────────────┐
     ┌───────────────────────┐   ┌──────────────────────────────────┐   │IMediaRepository              │   │IRatingRepository               │
     │Atom                   │   │IUserRepository                   │   ├──────────────────────────────┤   ├────────────────────────────────┤
     ├───────────────────────┤   ├──────────────────────────────────┤   │+ Add(media : Media) : void   │   │+ Add(rating : Rating) : void   │
     │- VerifySession()      │   │+ Add(user : User) : void         │   │+ Get(id : int) : Media       │   │+ Get(id : int) : Rating        │
     │- _EndEdit()           │   │+ Get(userName : string) : User   │   │+ Update(media : Media) : void│   │+ Update(rating : Rating) : void│
     │- _EnsureAdmin()       │   │+ Update(user : User) : void      │   │+ Delete(id : int) : void     │   │+ Delete(id : int) : void       │
     │- _EnsureAdminOrOwner()│   │+ Delete(userName : string) : void│   │+ GetAll()                    │   │+ GetAllForMedia(id : int)      │
     └───────────────────────┘   └──────────────────────────────────┘   │+ Clear()                     │   └────────────────────────────────┘
    -----------  | ---------------------------------        |           └──────────────────────────────┘                    |                
    |            |                                   |      |                           |                                   |                
    |     ┌────────────────────────────────────┐     |      |                           |                                   |                
    |     │User                                │     |      |                           |                                   |                
    |     ├────────────────────────────────────┤     |      |                           |                                   |                
    |     │- UserName : string                 │     |      |                           |                                   |                
    |     │- FullName : string                 │     |      |                           |                                   |                
    |     │- EMail : string                    │     |    ┌──────────────┐         ┌───────────────┐                ┌────────────────┐        
    |     │- PasswordHash : string             │     |    │UserRepository│         │MediaRepository│                │RatingRepository│        
    |     │--                                  │     |    ├──────────────┤         ├───────────────┤                ├────────────────┤        
    |     │+ SetPassword(pw : string) : void   │     |    └──────────────┘         └───────────────┘                └────────────────┘        
    |     │+ VerifyPassword(pw : string) : bool│     |                  |                 |                                  |                   
    |     │+ Delete() : void                   │     |                  |                 |                                  |                  
    |     │+ Save() : void                     │     |                  |                 |                                  |                   
    |     │+ Refresh() : void                  │     |                  |                 |                                  |                   
    |     └────────────────────────────────────┘     |                  |                 |                                  |                   
    |                                                |                  |                 |                                  |
    |                                                |                  |                 |                                  |
    |                                                |                  |                 |                                  |
    |                                                |                 ┌──────────────────────────────────────┐              |                   
    |                                                |                 │Repositories                          │              |                   
    |                                                |                 ├──────────────────────────────────────┤---------------                   
    |                                                |                 │{static} + Users : IUserRepository    │                                 
    |                                                |                 │{static} + Media : IMediaRepository   │                                 
    |                                                |                 │{static} + Ratings : IRatingRepository│                                 
    |                                                |                 └──────────────────────────────────────┘
    |                                                |
    |                                                |
    |                                                |
    |                                      ┌────────────────────────────────────────────┐                                                    
    |     ┌──────────────────────┐         │Media                                       │                                                    
    |     │Session               │         ├────────────────────────────────────────────┤                                                    
    |     ├──────────────────────┤         │- Id : int                                  │                                                    
    |     │- Token : string      │         │- Title : string                            │                                                    
    |     │- UserName : string   │         │- Description : string                      │                                                    
    |-----│- IsAdmin : bool      │         │- Type : string      ' Movie / Series / Game│                                                    
    |     │- Timestamp : DateTime│         │- ReleaseYear : int                         │                                                    
    |     │- Cleanup() : void    │         │- Genres : List<string>                     │                                                    
    |     │--                    │         │- AgeRestriction : int                      │                                                    
    |     │+ Valid() : bool      │         │- OwnerUserName: string                     │                                                    
    |     │+ IsAdmin(): bool     │         │--                                          │                                                    
    |     │+ Close() : void      │         │+ Delete() : void                           │                                                    
    |     └──────────────────────┘         │+ Save() : void                             │                                                    
    |                                      │+ Refresh() : void                          │                                                    
    |                                      └────────────────────────────────────────────┘                                                    
    |                                                                                                                                         
    |                               ┌────────────────────────────────┐                                                                        
    |                               │Rating                          │                                                                        
    |                               ├────────────────────────────────┤                                                                        
    |                               │- Id : int                      │                                                                        
    |                               │- MediaId: int                  │                                                                        
    |                               │- Creator: string               │                                                                        
    ------------------------------  │- Stars : int             ' 1..5│                                                                        
                                    │- Comment : string              │                                                                        
                                    │- Timestamp : DateTime          │                                                                        
                                    │--                              │                                                                        
                                    │+ Like() : void                 │                                                                        
                                    │+ Unlike() : void               │                                                                        
                                    │+ Delete() : void               │                                                                        
                                    │+ Save() : void                 │                                                                        
                                    │+ Refresh() : void              │                                                                        
                                    └────────────────────────────────┘
                                    
8. Zuerst wurde ein minimaler HttpRestServer implementiert, der mit HttpListener arbeitet und eingehende Requests in HttpRestEventArgs kapselt. Die Architektur wurde anschließend in klare Schichten aufgeteilt, um die Wartbarkeit zu erhöhen und dem SOLID-Prinzip gerecht zu werden. Insbesondere wurde das Single Responsibility Principle berücksichtigt (Server nur für HTTP, Handler nur für Routing/Businesslogik, Repositories nur für Datenzugriff). Die Entscheidung, Handler dynamisch mittels Reflection (Activator.CreateInstance) zu laden, reduziert Kopplung und erhöht die Erweiterbarkeit im Sinne des Open/Closed Principle. Neue Handler können hinzugefügt werden, ohne bestehende Klassen anzupassen. Tokens wurden als zufällige Strings implementiert, da dies für die Aufgabenstellung ausreichend und unkompliziert ist. Das Session-Management verwendet einen Lazy-Cleanup-Ansatz, der weniger komplex ist als ein timerbasierter Cleanup und gleichzeitig alle Anforderungen der Zwischenabgabe erfüllt. Für die Zwischenabgabe wurden In-Memory-Dictionaries verwendet. Die Architektur wurde jedoch bereits so gestaltet, dass der Austausch gegen eine PostgreSQL-Datenbank ohne Änderungen an der Geschäftslogik möglich ist. 

9. Erklärung der Unit-Tests & warum diese Logik getestet wurde
Aktuell wurden noch keine Unit-Tests durchgeführt

10. Probleme & deren Lösungen
- Media-Update ohne Ownership-Check. 
Lösung: _EnsureAdminOrOwner() in Atom.
- Es gab Routing-Konflikte bei ähnlichen Pfaden StartsWith("/users") kollidierte anfangs mit /users/profile und /users/login.
Lösung: Präziseres Pfad-Splitting (path.Split('/'))
- Token wurde nicht immer korrekt zurückgegeben. Nach Login wurde der Token teilweise nicht sauber in der Response gesetzt, weil Responded zu früh gesetzt wurde.
Lösung: Reihenfolge der Request-Verarbeitung angepasst.
- Der Token musste in Postman immer manuell kopiert werden um Requests mit Token durchzuführen.
Lösung: In Scirpts den Token in "authToken" speichern für alle weiteren Requests. 
var jsonData = pm.response.json();
pm.environment.set("authToken", jsonData.token);

11. Zeitaufwand pro Teilbereich (geschätzt)
- Server/Listener/Events ~ 4h
- Handler-System mit Reflection ~ 3h
- User-System (Register/Login/Hashing) ~ 6h
- Media-CRUD ~ 4h
- Session-System ~ 3h
- UML-Diagramme	~ 2h
- Postman-Collection ~ 3h
- Debugging & Fehlerbehebung ~ 5h
- Dokumentation ~ 3h

12. GitHub
https://github.com/Koalabot95/MRP.git
