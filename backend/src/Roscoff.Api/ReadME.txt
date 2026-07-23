comando da route progetto per migrazione: 

dotnet ef database update \ 
  --project src/Roscoff.Infrastructure \
  --startup-project src/Roscoff.Api
  
COmando di creazione migrazioni da route progetto: dotnet ef migrations add AddCatalogTables \ 
--project src/Roscoff.Infrastructure \
--startup-project src/Roscoff.Api