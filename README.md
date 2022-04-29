# NBPapi
Zaimplementuj mikroserwis, który pozwoli na przeliczanie walut zgodnie z kursem udostępnianym przez NBP.

Wytyczne:

- aplikacja powinna udostępniać REST API, która zwróci wszystkie (dostępne w ramach API NBP), trzyliterowe kody walut

- aplikacja powinna udostępniać REST API, które w parametrze przyjmie kod waluty oraz zwróci kurs danej waluty

- aplikacja powinna udostępniać REST API, które w parametrze przyjmie kod waluty oraz kwotę w danej walucie i przeliczy ją na złotówki

- udostępnione REST API, w przypadku podania niepoprawnych danych powinno zwrócić odpowiedni błąd

- aplikacja powinna przechowywać dane w cache

- jeżeli w przeciągu 5 minut nastąpi żądanie o pobrany już wcześniej obiekt, powinien on zostać załadowany z cache.

- czas życia obiektów w cache to 5 minut
