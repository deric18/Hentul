// See https://aka.ms/new-console-template for more information

using SchemaGenerator;

Console.WriteLine("Generting Schemas :");

SchemaGenerator.SchemaGenerator sg = new SchemaGenerator.SchemaGenerator();

//SOM : 200 * 10 * 5

Console.WriteLine("Press any key to generate Dendritic Schema :");

Console.ReadKey();

Console.WriteLine("Generating Dendritic Schema:");

sg.GenerateDendriticSchema();

Console.WriteLine("Done!");

Console.WriteLine("Press any key to generate Axonal Schema!");

Console.ReadKey();

Console.WriteLine("Generating Axonal Schema :");

sg.GenerateAxonalSchema();

Console.WriteLine("Done!.");