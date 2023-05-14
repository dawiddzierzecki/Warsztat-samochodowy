# Car workshop
The implemented program is a console application written in C#. The program uses multiple threads to simulate the operation of an auto repair shop. Each thread is an independent object performing its tasks. While the program is running, the console screen displays messages about the current actions performed by the processes. Slim semaphores are used in the program as objects for synchronizing access to critical sections. They provide access to a critical section only to one process at a time. The rest of the processes, waiting for access, queue in a FIFO. The use of semaphores avoids conflicts between processes, while ensuring the correct operation of the program. The program is a good example of how to apply synchronization mechanisms (in this case, semaphores) to build multithreaded, and therefore more efficient, applications in C#.

Assumptions:

In my workshop works:

- Receptionist - Coordinator - Diagnostic service - Service Mechanic 1 - Service Mechanic 2 - Order person - Paint service.

When a customer arrives at the workshop, he is received by the receptionist and put on a waiting list. From the waiting list, he is picked up by the coordinator, who is responsible for properly performing all the services needed. A car may need 3 types of services (diagnostics, mechanics, painting). Depending on the needs, the car is directed to the appropriate services, except that the order in which the customer's needs are checked is:

1. Diagnostics
2. Mechanics
3. Painting

Each service, after completing its service, places the car on a waiting list (in the internal affairs queue) to the coordinator. The coordinator decides what to do next. In addition, diagnostics, after completing its service, may determine that the car needs repair. Then it is referred to the coordinator, and the coordinator places it first in the queue for the mechanics' service. In addition, the mechanic, after diagnosis, may determine that additional parts are required for repair. Then the car is directed to the parking lot, and he submits the need to order parts to the order processor. When the parts arrive, the car is directed immediately (bypassing the coordinator) to the mechanics' queue as the highest priority object.


