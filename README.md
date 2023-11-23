# TheCardEditor

### Scaffold DB

```
Scaffold-DbContext "DataSource=C:\Users\GruselGusel\Desktop\Repos\TheCardEditor\data.sqlite3" Microsoft.EntityFrameworkCore.Sqlite -NoOnConfiguring -OutputDir ../TheCardEditor.DataModel/DataModel -Force -Context DataContext -Namespace TheCardEditor.DataModel.DataModel -StartupProject TheCardEditor.DataModel -Project TheCardEditor.DataModel
```
### Migrate DB

```
Add-Migration AddZoomToCardSet -Context TheCardEditor.DataModel.DataModel.DataContext -Project TheCardEditor.DataModel -StartupProject TheCardEditor.DataModel
```
