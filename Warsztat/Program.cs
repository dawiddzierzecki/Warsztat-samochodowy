/*
Programowanie współbieżne - projekt 04/2022 Serwis samochodowy
Autor: Dawid Dzierzęcki
Grupa: WCY21IX1N1
 */

using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Threading;

class Program
{
    //Zmienne do zarządzania programem - można modyfikowac wedle upodobania
    static int carsAmount = 20;                  //Liczba samochodów jaka ma zostać wygenerowana
    static int customerFrequency = 2000;              //Częstotliwośc z jaką mają być generowani nowi klienci
    static int breakDuration = 2000;                //Czas trwania przerwy pracownika
    static int coordinatorInternalDuration = 1000;  //Czas trwania obsługi wewnętrzengo zgłoszenia przez koordynatora
    static int coordinatorClientDuration = 1000;    //Czas trwania obsługi klienta przez koordynatora (pierwszy kontakt)
    static int diagnosticsDuration = 3000;          //Czas trwania usługi diagnostycznej 
    static int mechanicalDuration = 3000;           //Czas trwania naprawy mechanicznej 
    static int partsDuration = 3000;                //Czas trwania dostarczenia części do naprawy
    static int paintingDuration = 5000;             //Czas trwania malowania samochodu

    //Listy - w praktyce kolejki do danego oddziału (koordynatora, diagnostyki, mechaniki etc.)
    static List<Car> customerList = new List<Car>();
    static List<Car> internalList = new List<Car>();
    static List<Car> diagnosticsList = new List<Car>();
    static List<Car> mechanicalList = new List<Car>();
    static List<Car> partsOrderList = new List<Car>();
    static List<Car> paintingServiceList = new List<Car>();

    //Semafory do synchronizacji dostępu do list (kolejek) oraz ekranu
    static SemaphoreSlim customerSemaphore = new SemaphoreSlim(1, 1);
    static SemaphoreSlim diagnosticsSemaphore = new SemaphoreSlim(1, 1);
    static SemaphoreSlim mechanicalSemaphore = new SemaphoreSlim(1, 1);
    static SemaphoreSlim partsOrderSemaphore = new SemaphoreSlim(1, 1);
    static SemaphoreSlim paintingServiceSemaphore = new SemaphoreSlim(1, 1);
    static SemaphoreSlim internalListSemaphore = new SemaphoreSlim(1, 1);
    static SemaphoreSlim screenSemaphore = new SemaphoreSlim(1, 1);


    static void Main()
    {
        Thread customerThread = new Thread(CustomerThread);
        Thread coordinatorThread = new Thread(CoordinatorThread);
        Thread diagnosticsThread = new Thread(DiagnosticsThread);
        Thread mechanicalThread = new Thread(MechanicalThread);
        Thread mechanical2Thread = new Thread(Mechanical2Thread);
        Thread partsOrderThread = new Thread(PartsOrderThread);
        Thread paintingServiceThread = new Thread(PaintingServiceThread);

        customerThread.Start();
        coordinatorThread.Start();
        diagnosticsThread.Start();
        mechanicalThread.Start();
        mechanical2Thread.Start();
        partsOrderThread.Start();
        paintingServiceThread.Start();

        //Oczekiwanie na zakończenie wątków. (W praktyce nigdy się nie skończą - brak warunku)
        customerThread.Join();
        coordinatorThread.Join();
        diagnosticsThread.Join();
        mechanicalThread.Join();
        mechanical2Thread.Join();
        partsOrderThread.Join();
        paintingServiceThread.Join();

        Console.WriteLine("All work is done.");
        Console.WriteLine("I'm closing the workshop...");
    }

    //Wątek tworzący klientów (dodaje samochody do kolejki oczekujących customerList)
    static void CustomerThread()
    {
        for (int i = 0; i < carsAmount; i++)
        {
            Car car = new Car(i + 1);
            screenSemaphore.Wait();
            Console.WriteLine("Customer {0} arrived.", car.number);
            screenSemaphore.Release();
            customerSemaphore.Wait();
            customerList.Add(car);
            customerSemaphore.Release();
            Thread.Sleep(customerFrequency);
        }
    }

    //Wątek koordynatora - to on zarządza całym warsztatem. Kieruje autami tak, aby trafiły w odpowiednie miejsca.
    static void CoordinatorThread()
    {
        while (true)
        {
            //Obsługa spraw wewnętrznych (te auta, które już u niego były) ma priorytet
            internalListSemaphore.Wait();
            if(internalList.Count == 0)
            {
                internalListSemaphore.Release();
                screenSemaphore.Wait();
                Console.WriteLine("\tCOORDINATOR: No internal jobs. Going to clients.");
                screenSemaphore.Release();
            } 
            else
            {
                Car car = internalList[0];
                internalList.RemoveAt(0);
                internalListSemaphore.Release();
                if (car.needsDiagnostics)
                {
                    screenSemaphore.Wait();
                    Console.WriteLine("\tCOORDINATOR: Car number {0} is going for diagnostics...", car.number);
                    screenSemaphore.Release();
                    Thread.Sleep(coordinatorInternalDuration);
                    diagnosticsSemaphore.Wait();
                    diagnosticsList.Add(car);
                    diagnosticsSemaphore.Release();
                }
                else if (car.needsMechanicalWork)
                {
                    screenSemaphore.Wait();
                    Console.WriteLine("\tCOORDINATOR: !PRIORITY! Car number {0} is going for mechanic service...", car.number);
                    screenSemaphore.Release();
                    Thread.Sleep(coordinatorInternalDuration);
                    mechanicalSemaphore.Wait();
                    mechanicalList.Insert(0, car);
                    mechanicalSemaphore.Release();
                }
                else if (car.needsPainting)
                {
                    screenSemaphore.Wait();
                    Console.WriteLine("\tCOORDINATOR: Car number {0} goes to painting service...", car.number);
                    screenSemaphore.Release();
                    Thread.Sleep(coordinatorInternalDuration);
                    paintingServiceSemaphore.Wait();
                    paintingServiceList.Add(car);
                    paintingServiceSemaphore.Release();
                }
                else
                {
                    screenSemaphore.Wait();
                    Console.WriteLine("COORDINATOR: No more action needed for car number {0}. Bye bye!", car.number);
                    screenSemaphore.Release();
                }
            }
            
            //Obsługa klientów
            customerSemaphore.Wait();
            if (customerList.Count == 0)
            {
                customerSemaphore.Release();
                screenSemaphore.Wait();
                Console.WriteLine("\tCOORDINATOR: No clients in queue. Going smoke...");
                screenSemaphore.Release();
                Thread.Sleep(breakDuration);
                continue;
            }
            else
            {
                Car car = customerList[0];
                customerList.RemoveAt(0);
                customerSemaphore.Release();
                if (car.needsDiagnostics)
                {
                    screenSemaphore.Wait();
                    Console.WriteLine("\tCOORDINATOR: Customer's {0} car is going for diagnostics...", car.number);
                    screenSemaphore.Release();
                    Thread.Sleep(coordinatorClientDuration);
                    diagnosticsSemaphore.Wait();
                    diagnosticsList.Add(car);
                    diagnosticsSemaphore.Release();
                }
                else if (car.needsMechanicalWork)
                {
                    screenSemaphore.Wait();
                    Console.WriteLine("\tCOORDINATOR: Customer's {0} car is going for mechanic service...", car.number);
                    screenSemaphore.Release();
                    Thread.Sleep(coordinatorClientDuration);
                    mechanicalSemaphore.Wait();
                    mechanicalList.Add(car);
                    mechanicalSemaphore.Release();
                }
                else if (car.needsPainting)
                {
                    screenSemaphore.Wait();
                    Console.WriteLine("\tCOORDINATOR: Customer's {0} car goes to painting service...", car.number);
                    screenSemaphore.Release();
                    Thread.Sleep(coordinatorClientDuration);
                    paintingServiceSemaphore.Wait();
                    paintingServiceList.Add(car);
                    paintingServiceSemaphore.Release();
                }
                else
                {
                    screenSemaphore.Wait();
                    Console.WriteLine("COORDINATOR: Client {0} - no action needed for you. Why are you here?!", car.number);
                    screenSemaphore.Release();
                }
            }
        }    
    }

    //Wątek serwisu diagnostycznego
    static void DiagnosticsThread()
    {
        while (true)
        {
            diagnosticsSemaphore.Wait();
            if (diagnosticsList.Count == 0)
            {
                diagnosticsSemaphore.Release();
                screenSemaphore.Wait();
                Console.WriteLine("\tDIAGNOSTICS: No cars to diagnose. Going smoke...");
                screenSemaphore.Release();
                Thread.Sleep(breakDuration);
                continue;
            }
            Car car = diagnosticsList[0];
            diagnosticsList.RemoveAt(0);
            diagnosticsSemaphore.Release();
            screenSemaphore.Wait();
            Console.WriteLine("\tDIAGNOSTICS: Diagnostics in progress for car number {0}...", car.number);
            screenSemaphore.Release();
            Thread.Sleep(diagnosticsDuration);
            car.needsDiagnostics = false;
            screenSemaphore.Wait();
            Console.WriteLine("\tDIAGNOSTICS: Diagnostics for car number {0} is end.", car.number);
            screenSemaphore.Release();
            internalListSemaphore.Wait();
            internalList.Add(car);
            internalListSemaphore.Release();
        }
    }

    //Wątek mechanika nr 1
    static void MechanicalThread()
    {
        while (true)
        {
            mechanicalSemaphore.Wait();
            if (mechanicalList.Count == 0)
            {
                mechanicalSemaphore.Release();
                screenSemaphore.Wait();
                Console.WriteLine("\tMECHANIC 1: Don't have any cars to repair. Going smoke...");
                screenSemaphore.Release();
                Thread.Sleep(breakDuration);
                continue;
            }
            Car car = mechanicalList[0];
            mechanicalList.RemoveAt(0);
            mechanicalSemaphore.Release();
            screenSemaphore.Wait();
            Console.WriteLine("\tMECHANIC 1: Mechanical work in progress for customer's {0} car...", car.number);
            screenSemaphore.Release();
            Thread.Sleep(mechanicalDuration);
            if (car.needsParts)
            {
                screenSemaphore.Wait();
                Console.WriteLine("\tMECHANIC 1: car number {0} needs additional parts to repair. Car goes to the parking lot...", car.number);
                screenSemaphore.Release();
                partsOrderSemaphore.Wait();
                partsOrderList.Add(car);
                partsOrderSemaphore.Release();
            }
            else
            {
                car.needsMechanicalWork = false;
                screenSemaphore.Wait();
                Console.WriteLine("\tMECHANIC 1: Car number {0} was fixed.", car.number);
                screenSemaphore.Release();
                internalListSemaphore.Wait();
                internalList.Add(car);
                internalListSemaphore.Release();
            }
        }
    }

    //Wątek mechanika nr 2
    static void Mechanical2Thread()
    {
        while (true)
        {
            mechanicalSemaphore.Wait();
            if (mechanicalList.Count == 0)
            {
                mechanicalSemaphore.Release();
                screenSemaphore.Wait();
                Console.WriteLine("\tMECHANIC 2: Don't have any cars to repair. Going smoke...");
                screenSemaphore.Release();
                Thread.Sleep(breakDuration);
                continue;
            }
            Car car = mechanicalList[0];
            mechanicalList.RemoveAt(0);
            mechanicalSemaphore.Release();
            screenSemaphore.Wait();
            Console.WriteLine("\tMECHANIC 2: Mechanical work in progress for customer's {0} car...", car.number);
            screenSemaphore.Release();
            Thread.Sleep(mechanicalDuration);
            if (car.needsParts)
            {
                screenSemaphore.Wait();
                Console.WriteLine("\tMECHANIC 2: car number {0} needs additional parts to repair. Car goes to the parking lot...", car.number);
                screenSemaphore.Release();
                partsOrderSemaphore.Wait();
                partsOrderList.Add(car);
                partsOrderSemaphore.Release();
            }
            else
            {
                car.needsMechanicalWork = false;
                screenSemaphore.Wait();
                Console.WriteLine("\tMECHANIC 2: Car number {0} was fixed.", car.number);
                screenSemaphore.Release();
                internalListSemaphore.Wait();
                internalList.Add(car);
                internalListSemaphore.Release();
            }
        }
    }

    //Wątek zamawiający części do naprawy
    static void PartsOrderThread()
    {
        while (true)
        {
            partsOrderSemaphore.Wait();
            if (partsOrderList.Count == 0)
            {
                partsOrderSemaphore.Release();
                screenSemaphore.Wait();
                Console.WriteLine("\tPARTS: I don't have any parts to order. Going smoke...");
                screenSemaphore.Release();
                Thread.Sleep(breakDuration);
                continue;
            }
            Car car = partsOrderList[0];
            partsOrderList.RemoveAt(0);
            partsOrderSemaphore.Release();
            screenSemaphore.Wait();
            Console.WriteLine("\tPARTS: I ordered parts for car number {0}...", car.number);
            screenSemaphore.Release();
            Thread.Sleep(partsDuration);
            screenSemaphore.Wait();
            Console.WriteLine("\tPARTS: Additional parts for car number {0} arrived.", car.number);
            Console.WriteLine("\tPARTS: !PRIORITY! Car {0} goes for repair.", car.number);
            screenSemaphore.Release();
            car.needsParts = false;
            mechanicalSemaphore.Wait();
            mechanicalList.Insert(0, car);
            mechanicalSemaphore.Release();
        }
    }

    //wątek serwisu blacharsko - lakierniczego
    static void PaintingServiceThread()
    {
        while (true)
        {
            paintingServiceSemaphore.Wait();
            if (paintingServiceList.Count == 0)
            {
                paintingServiceSemaphore.Release();
                screenSemaphore.Wait();
                Console.WriteLine("\tPAINTING SERVICE: Don't have any cars to paint. Going smoke...");
                screenSemaphore.Release();
                Thread.Sleep(breakDuration);
                continue;
            }
            Car car = paintingServiceList[0];
            paintingServiceList.RemoveAt(0);
            paintingServiceSemaphore.Release();
            screenSemaphore.Wait();
            Console.WriteLine("\tPAINTING SERVICE: Painting car number {0} in progress...", car.number);
            screenSemaphore.Release();
            Thread.Sleep(paintingDuration);
            screenSemaphore.Wait();
            Console.WriteLine("\tPAINTING SERVICE: Painting car number {0} was finished.", car.number);
            screenSemaphore.Release();
        }
    }
}