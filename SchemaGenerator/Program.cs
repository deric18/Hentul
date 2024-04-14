// See https://aka.ms/new-console-template for more information

using SchemaGenerator;


Console.WriteLine("Generting Schemas :");


SchemaGenerator.SchemaGenerator sg = new SchemaGenerator.SchemaGenerator(100, 10, 10, 2);

Console.WriteLine("Press any key to generate Dendritic Schema :");

Console.ReadKey();

Console.WriteLine("Generating Dendritic Schema:");

sg.GenerateDendriticSchema();

Console.WriteLine("Done!");

Console.WriteLine("Press any key to generate Axonal Schema!");

Console.ReadKey();

sg.GenerateAxonalSchema();

Console.WriteLine("Generating Axonal Schema :");
