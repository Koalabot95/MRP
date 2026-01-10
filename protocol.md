MRP Zwischenabgabe - Projektdokumentation

1. Ueberblick

Dieses Projekt implementiert einen HTTP-REST-Server in C#. Der Server unterstützt vollständige CRUD-Operationen für Benutzer (Registrierung, Login mit Bearer-Token-Authentifizierung, Aktualisieren und Löschen von Benutzerdaten) sowie für Media-Einträge. Media-Einträge können bewertet (Ratings), kommentiert und als Favoriten markiert werden. Die Authentifizierung erfolgt über Bearer Tokens, wobei Sessions und Tokens serverseitig im Arbeitsspeicher (RAM) verwaltet werden. 

Je nachdem ob der Postgres-Container aktiv ist, werden User/Media Einträge lokal im RAM verwaltet oder in Postgres.

2. Architektur

2.1 Listener & Dispatcher
Der HttpListener nimmt eingehende Requests entgegen und wandelt sie in HttpRestEventArgs e Objekte um. Der Server leitet jedes Request-Objekt an alle registrierten Handler weiter, wobei jeder Handler selbst prüft, ob er für den jeweiligen Request zuständig ist.


2.2 Handler-Konzept
Durch das Zerlegen des URL-Pfads ist ein einfaches Routing möglich. Jeder Handler prueft anhand des Request-Pfads, ob er zustaendig ist (z. B. e.Path.StartsWith("/users")).

2.3 Session- und Token-System
Nach erfolgreichem Login wird eine Session erzeugt, die Username, Token und Timestamp enthaelt. Tokens werden in einem Dictionary gespeichert. Lazy-Cleanup entfernt abgelaufene Sessions. Authentifizierte Endpoints nutzen GetSessionOrThrow zur Token-Pruefung. Die Token basieren auf 24 zufaelligen Zeichen. Der Zugriff auf geschützte Endpoints erfolgt ausschließlich über einen gültigen Bearer Token. Die Sessions verfallen ueber Cleanup oder wenn das Programm geschlossen wird. Der Session-Timestamp wird bei jeder erfolgreichen Verwendung aktualisiert.

2.4 Postgres
Die Anwendung nutzt PostgreSQL als relationale Datenbank und wird über Docker betrieben. Abhängig von der Konfiguration werden Benutzer-, Media-, Rating- und Favoriten-Daten entweder in einer PostgreSQL-Datenbank oder alternativ im Arbeitsspeicher (In-Memory-Repositories) gespeichert. Sessions und Tokens werden unabhängig davon ausschließlich im RAM verwaltet.

3. Domain-Modelle

3.1 User  
Enthält UserName, FullName, Email und PasswordHash (SHA256). Der UserName ist nach der Registrierung unveränderbar und dient als eindeutiger Identifikator.

3.2 Media  
Enthält Id, Title, Description, Type, ReleaseYear, Genres, AgeRestriction und OwnerUserName. Media-Einträge können bewertet und als Favoriten markiert werden.

3.3 Rating  
Ein Rating repräsentiert eine Bewertung eines Media-Eintrags durch einen Benutzer. Jedes Rating ist eindeutig einem Media-Eintrag und einem Benutzer zugeordnet und enthält eine Sternebewertung (1–5), einen optionalen Kommentar sowie einen Zeitstempel.

Ratings können ausschließlich vom Ersteller bearbeitet oder gelöscht werden. Ratings können von anderen Benutzern geliked werden. Likes werden nicht als Attribut des Ratings gespeichert, sondern als relationale Zuordnung zwischen Benutzern und Ratings, um Mehrfachlikes zu verhindern.

3.4 Favorites  
Favorites modellieren die Beziehung zwischen Benutzern und Media-Einträgen. Benutzer können Media-Einträge als Favoriten markieren und über den Endpoint "/favorites" ihre persönlichen Favoriten abrufen. Favorites besitzen keine eigene Domain-Entität, sondern werden als relationale
Zuordnung zwischen User und Media implementiert.
 
4. Repositories
Je nach Konfiguration arbeiten die Repositories entweder mit In-Memory-Datenstrukturen oder mit einer PostgreSQL-Datenbank. Implementiert sind UserRepository, MediaRepository, RatingRepository und FavoriteRepository bzw. PostgresUserRepo, PostgresMediaRepo, PostgresRatingRepo und PostgresFavoriteRepo.

6. Integration Tests
Fuer die Abgabe wurde eine Postman Collection mit allen Endpoints bereitgestellt.

7. UML-Diagramme

7.1 Architektur
Im Abgabe Ordner bereitgestellt.

7.2 Domänen + Persistenz
Im Abgabe Ordner bereitgestellt.
                                    
8. Decisions
Zuerst wurde ein minimaler HttpRestServer implementiert, der mit HttpListener arbeitet und eingehende Requests in HttpRestEventArgs kapselt. Die Architektur wurde anschließend in klare Schichten aufgeteilt, um die Wartbarkeit zu erhöhen und dem SOLID-Prinzipien gerecht zu werden. Insbesondere wurde das Single Responsibility Principle berücksichtigt (Server nur für HTTP, Handler nur für Routing/Businesslogik, Repositories nur für Datenzugriff). Die Entscheidung, Handler dynamisch mittels Reflection (Activator.CreateInstance) zu laden, reduziert Kopplung und erhöht die Erweiterbarkeit im Sinne des Open/Closed Principle. Neue Handler können hinzugefügt werden, ohne bestehende Klassen anzupassen. Tokens wurden als zufällige Strings implementiert, da dies für die Aufgabenstellung ausreichend und unkompliziert ist. Das Session-Management verwendet einen Lazy-Cleanup-Ansatz, der weniger komplex ist als ein timerbasierter Cleanup und gleichzeitig alle Anforderungen der Abgabe erfüllt. Für die Zwischenabgabe wurden In-Memory-Dictionaries verwendet. Die Architektur wurde jedoch bereits so gestaltet, dass der Austausch gegen eine PostgreSQL-Datenbank ohne Änderungen an der Geschäftslogik möglich war. Es wurden nach der Datenbankanbindung noch fehelende Repos ergänzt, was problemlos möglich war. Weiters wurde die Entscheidung getroffen, die bestehenden InMemory-Repos zu behalten und je nach Konfiguration dann diese zu verwenden oder auf Postgres umzuleiten.

9. Erklärung der Unit-Tests & warum diese Logik getestet wurde
Ziel der Unit-Tests ist es, die fachliche Logik der Domänenobjekte und Repositories isoliert zu prüfen. Die Tests zielen auf InMemory-Repositories ab, da die PostgreSQL-Implementierungen über Integrationstests abgedeckt werden.

Geteste Logik:
9.1 User (UserNamenTests und UserPasswordTests): Passwort und Identität

Passwort-Hashing/VerifyPassword: Es wird geprüft, dass Passwörter nicht im Klartext gespeichert und korrekt verifiziert werden, um Login-Probleme und Sicherheitslücken vorzubeugen

UserName: Testet, dass UserName nach Registrierung nicht geändert werden kann, sowie korrektes Anlegen des Usernamen. Dies ist eine konkrete Anforderung, da er unter anderem als Schlüssel zur Zuordnung von Ratings/Favorites/Likes dient.


9.2 Media (MediaTests): CRUD-Grundfunktionen + Owner-Konzept

Add/Get/Update/Delete prüft die korrekte Speicherung, Überschreiben und Entfernen InMemory. Dies ist eine essentielle Basisfunktion, auf der fast alle Endpoints aufbauen.

Berechtigung (Admin/Owner) beim Editieren/Löschen von Einträgen. Stellt sicher, dass Nutzer fremde Medien manipulieren.

9.3 Rating (RatingStatsTests, RatingRegelnTests): Validierung und Ownership

Stars-Validierung (1–5): Testet, dass ungültige Werte (0, 6, negativ) eine Exception auslösen, gültige Werte akzeptiert werden sowie das ein Kommentar optional ist. Dies dient dazu, dass die Bewertung korrekt und konsistent läuft.

Average: Testet verschiedene Cases zur Berechnung der durchschnittlichen Bewertung.

9.4 RatingLikes (RatingLikeTests): Relation und Idempotenz

Like ist pro User/Rating nur einmal möglich: wiederholtes Liken darf keinen Zähler erhöhen oder muss abgewiesen werden. Verhindert mehrfaches Liken des Ratings. Ebenso wurde getestet ob ein Unlike korrekt funktioniert.

9.5 Authentifizierung (AtomAuthTests)

Berechtigungen (Admin/Owner): Hier werden allgemein die Admin/Owner funktionen getestet um einen korrekten Zugriff zu gewähren.

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

- Fehler bei Unit-Test: zugriff auf User_HashPassword war aufgrund des Schutzgrades nicht möglich.
Lösung: AssemblyInfo.cs Datei, welche den Zugriff dem Testprojekt erlaubt.

- Bei Ratingeintrag hat immer der MediaHanlder übernommen.
Lösung: statt string idStr = e.Path.Substring("/media/".Length);
-> string rest = e.Path.Substring("/media/".Length);   // z.B. "1/ratings"
string idPart = rest.Split('/', 2)[0];              // -> "1"
sowie if (rest.Contains("/")) return; ergänt. Leitet jetzt an RatingHandler weiter.

- Zu Beginn befand sich Logik zur Validierung (z. B. Sternebereich 1–5) teilweise im Repository.
Lösung: Verschiebung dieser Regeln in die Domänenklassen (Rating, Media), sodass Repositories ausschließlich für Datenzugriff zuständig sind.

- Likes waren zunächst als Methode in Rating implementiert, während ich eine relationale Tabelle (rating_likes) implementierte.
Lösung: Entscheidung für eine klare Repository-basierte Lösung (RatingLikeRepository), da Likes eine Many-to-Many-Beziehung darstellen.

- Wiederholte Like-Requests desselben Users führten anfangs zu mehrfacher Zählung.
Lösung: Eindeutige Prüfung im Repository (Unique-Constraint bzw. Exception), sodass ein User ein Rating nur einmal liken kann.

- Anfangs wurde die Session direkt in mehreren Handlern geprüft und ausgewertet.
Lösung: Zentralisierung der Sicherheits- und Berechtigungslogik in der Basisklasse Atom, wodurch doppelter Code vermieden wurde.

- Erste Implementierungen enthielten Datenbank-spezifische Annahmen in der Geschäftslogik.
Lösung: Einführung klarer Repository-Interfaces und einer zentralen Repositories-Factory, die je nach Konfiguration die passende Implementierung bereitstellt.

- Da  die Repositories statisch sind, beeinflussten sich Tests gegenseitig.
Lösung: Vor jedem Test gezieltes Clear() der Repositories, um reproduzierbare Testergebnisse sicherzustellen.

11. Zeitaufwand pro Teilbereich (geschätzt)
- Server/Listener/Events ~ 4h
- Handler-System mit Reflection ~ 3h
- User-System (Register/Login/Hashing) ~ 6h
- Media-CRUD ~ 4h
- Rating-System (Bewertungen, Kommentare) ~ 4 h
- Favorites-System ~ 2 h
- Rating-Likes ~ 2 h
- Repository(Interfaces + InMemory/Postgres-Switch) ~ 3 h
- PostgreSQL-Anbindung (Docker, Mapping, Constraints) ~ 4 h
- Session-System ~ 3h
- UML-Diagramme	~ 2h
- Unit-Tests (Domain & Repositories) ~ 3 h
- Refactoring ~ 4 h
- Integrationstests (Postman Collection) ~ 3 h
- Debugging & Fehlerbehebung ~ 6h
- Dokumentation ~ 3h


12. GitHub
https://github.com/Koalabot95/MRP.git
