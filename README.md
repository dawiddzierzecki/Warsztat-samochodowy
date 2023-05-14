# Warsztat-samochodowy
Zaimplementowany program jest aplikacją konsolową napisaną w języku C#. Program korzysta z wielu wątków, aby zasymulować działanie warsztatu samochodowego. Każdy wątek to niezależny obiekt realizujący swoje zadania. W trakcie działania programu, na ekranie konsoli, wyświetlane są komunikaty o bieżących czynnościach wykonywanych przez dane procesy.
W programie zastosowano semafory typu Slim jako obiekty synchronizacji dostępu do sekcji krytycznych. Zapewniają one dostęp do sekcji krytycznej tylko i wyłącznie jednemu procesowi jednocześnie. Reszta procesów, oczekujących na dostęp, ustawia się w kolejkę FIFO. Zastosowanie semaforów pozwala uniknąć konfliktów pomiędzy procesami, jednocześnie zapewniając poprawne działanie programu.
Program jest dobrym przykładem jak zastosować mechanizmy synchronizacji (w tym przypadku semafory) do budowania wielowątkowych, co za tym idzie, wydajniejszych aplikacji w języku C#.

Założenia:

W moim warsztacie pracuje:

• Recepcjonistka
• Koordynator
• Serwis diagnostyczny
• Serwis Mechanik 1
• Serwis Mechanik 2
• Osoba od zamówień
• Serwis Lakierniczy

Kiedy klient przyjeżdża do warsztatu przyjmuje go recepcjonistka i umieszcza na liście oczekujących. Z listy oczekujących odbiera go koordynator, który jest odpowiedzialny za prawidłowe wykonanie wszystkich potrzebnych usług. Samochód może potrzebować 3 rodzajów usług (diagnostyka, mechanika, malowanie). W zależności od potrzeb samochód jest kierowany do odpowiednich serwisów, z tym że kolejność sprawdzania potrzeb klienta to:
1. Diagnostyka
2. Mechanika
3. Malowanie

Każdy serwis po zrealizowaniu swojej usługi umieszcza samochód na liście oczekujących (w kolejce spraw wewnętrznych) do koordynatora. Koordynator decyduje co dalej.
Ponadto, diagnostyka, po zrealizowaniu swojej usługi, może stwierdzić, że samochód wymaga naprawy. Wtedy jest on kierowany do koordynatora, a ten umieszcza go jako pierwszego w kolejce do serwisu mechaników.
Ponadto, mechanik po diagnozie może stwierdzić, że do naprawy wymagane są dodatkowe części. Wtedy samochód kierowany jest na parking, a on składa potrzebę zamówienia części do osoby realizującej zamówienia. Kiedy części przyjdą, samochód jest kierowany od razu (z pominięciem koordynatora) do kolejki mechaników jako obiekt z najwyższym priorytetem.

