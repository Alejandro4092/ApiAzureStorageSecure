# ApiAzureStorageSecure — Guía de Examen

Proyecto API REST en **.NET 10** que combina los dos proyectos de referencia:

| Proyecto referencia      | Qué aporta                                    |
|--------------------------|-----------------------------------------------|
| `ApiOauthEmpleados`      | JWT Bearer Token, Key Vault, patrón Helpers   |
| `MvcCoreAzureStorage`    | Azure Blob Storage, Azure Files, patrón Services |

---

## Estructura del proyecto

```
ApiAzureStorageSecure/
│
├── Controllers/
│   ├── AuthController.cs          ← POST /api/auth/login → devuelve JWT
│   ├── UsersController.cs         ← GET /api/users (público) + perfil/adminonly (con token)
│   ├── StorageBlobController.cs   ← CRUD Azure Blob Storage (Bearer requerido)
│   └── StorageFilesController.cs  ← CRUD Azure Files / File Share (Bearer requerido)
│
├── Helpers/
│   ├── HelperActionOAuthService.cs ← Configuración JWT: issuer, audience, key, opciones
│   ├── HelperCifrado.cs            ← Cifrado AES-256 del payload del token
│   └── HelperUserToken.cs          ← Lee UserModel cifrado del claim "UserData"
│
├── Models/
│   ├── LoginModel.cs     ← Body del POST /login
│   ├── UserModel.cs      ← Usuario que viaja cifrado en el JWT
│   └── BlobModel.cs      ← Representación de un blob listado
│
├── Repositories/
│   └── RepositoryUsers.cs  ← Simulación de BD de usuarios (en memoria)
│
├── Services/
│   ├── ServiceStorageBlobs.cs  ← Operaciones sobre Azure Blob Storage
│   └── ServiceStorageFiles.cs  ← Operaciones sobre Azure File Share
│
├── Program.cs              ← Registro DI + Key Vault + JWT + Blob + Files
├── appsettings.json        ← Config local (valores reales en Key Vault)
└── ApiAzureStorageSecure.http  ← Fichero de pruebas HTTP
```

---

## Flujo de seguridad (Bearer Token)

```
1. Cliente → POST /api/auth/login { userName, password }
2. API verifica credenciales (RepositoryUsers)
3. API cifra UserModel con AES-256 (HelperCifrado)
4. API genera JWT con el UserModel cifrado en claim "UserData" + claim Role
5. API devuelve { "response": "eyJhbGci..." }

6. Cliente → GET /api/storageblob/containers
             Authorization: Bearer eyJhbGci...
7. Middleware JWT valida firma, issuer, audience y expiración
8. Controller accede al usuario: helperToken.GetUser() → descifra claim → UserModel
```

---

## Azure Key Vault

Los secretos que guarda el Key Vault:

| Nombre del secreto          | Qué es                                      |
|-----------------------------|---------------------------------------------|
| `storageconnectionstring`   | Cadena de conexión del Storage Account      |
| `jwtkey`                    | Clave secreta para firmar los JWT           |

En `Program.cs` se recuperan así:
```csharp
SecretClient secretClient = builder.Services.BuildServiceProvider().GetService<SecretClient>();
KeyVaultSecret secreto = await secretClient.GetSecretAsync("storageconnectionstring");
string valor = secreto.Value;
```

**Para que funcione en local**, autentícate con Azure CLI:
```bash
az login
```

---

## Azure Blob Storage — Endpoints

| Método | Ruta                              | Auth   | Rol mínimo    |
|--------|-----------------------------------|--------|---------------|
| GET    | /api/storageblob/containers       | Bearer | cualquiera    |
| POST   | /api/storageblob/createcontainer  | Bearer | ADMIN         |
| DELETE | /api/storageblob/deletecontainer  | Bearer | ADMIN         |
| GET    | /api/storageblob/blobs            | Bearer | cualquiera    |
| GET    | /api/storageblob/downloadblob     | Bearer | cualquiera    |
| POST   | /api/storageblob/uploadblob       | Bearer | cualquiera    |
| DELETE | /api/storageblob/deleteblob       | Bearer | ADMIN/MANAGER |

---

## Azure Files — Endpoints

| Método | Ruta                               | Auth   | Rol mínimo |
|--------|------------------------------------|--------|------------|
| GET    | /api/storagefiles/files            | Bearer | cualquiera |
| GET    | /api/storagefiles/readfile         | Bearer | cualquiera |
| GET    | /api/storagefiles/downloadfile     | Bearer | cualquiera |
| POST   | /api/storagefiles/uploadfile       | Bearer | cualquiera |
| DELETE | /api/storagefiles/deletefile       | Bearer | ADMIN      |

---

## Pasos para ejecutar

1. **Rellenar appsettings.json** con tus valores de Azure:
   - `AzureKeys:StorageAccount` → cadena de conexión del Storage Account
   - `AzureKeys:ShareName`      → nombre del File Share
   - `KeyVault:VaultUri`        → URI del Key Vault

2. **Iniciar sesión en Azure** (para Key Vault con Managed Identity local):
   ```bash
   az login
   ```

3. **Ejecutar**:
   ```bash
   dotnet run
   ```

4. **Abrir** `https://localhost:7001/scalar` para la interfaz interactiva.

5. **Hacer login** con `POST /api/auth/login` y copiar el token.

6. En Scalar, pulsar el candado 🔒 → pegar el token → ejecutar los endpoints.

---

## Usuarios de prueba (en memoria)

| UserName  | Password | Rol     |
|-----------|----------|---------|
| admin     | 1234     | ADMIN   |
| usuario   | 5678     | USER    |
| manager   | 9012     | MANAGER |
