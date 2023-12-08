using Utility;

namespace Server;

internal record ThreadContainer(
    MultiplicationData Data,
    double[][] Result,
    CountdownEvent CountdownEvent,
    Stopper Stopper);
