using Xunit;

// De pipeline-/notificatie-stubs schrijven naar de console. Bij parallelle tests kan de
// door de testhost vastgelegde Console-writer al gesloten zijn, wat tot een
// ObjectDisposedException leidt. Sequentieel draaien maakt de tests deterministisch.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
