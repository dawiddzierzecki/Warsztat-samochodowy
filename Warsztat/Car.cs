class Car
{
    // Zmienne opisujące stan auta
    // Auto może potrzebować jednego z 3 rodzajów usług (diagnostyka, serwis mechaniczny, malowanie)
    public bool needsDiagnostics;
    public bool needsMechanicalWork;
    public bool needsPainting;
    
    public bool needsParts; // Zmienna definiująca czy jeśli auto jest do naprawy to czy potrzebuje dodatkowych części
    public int number;      // Numer porządkowy samochodu

    public Car(int i)
    {
        needsDiagnostics = new Random().Next(2) == 0;
        needsMechanicalWork = new Random().Next(2) == 0;
        needsParts = needsMechanicalWork && new Random().Next(2) == 0;
        needsPainting = new Random().Next(2) == 0;
        number = i;
    }
}