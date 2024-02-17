## Agregar saludos

"Saludos" se refiere al texto que los PNJs usan al pasar cerca de otro.
Puede ocurrir, por ejemplo, cuando dos PNJs se ven camino a sus rutinas.

Los saludos usan el archivo `mistyspring.dynamicdialogues/Greetings`.

No tiene un formato específico- sólo el nombre interno del PNJ, y las entradas del PNJ + su diálogo.

Plantilla:
```
      "PNJ_que_saluda": {
          "PNJ_A": "",
          "PNJ_B": "",
          "PNJ_C": ""
          //...etc. puedes agregar a tantos PNJs como quieras
        }    
```

**Ejemplo:**
Esto editará el saludo de Alex hacia Evelyn y George. Si los ve mientras camina a algún lado, puede que diga "Hola"(a Evelyn), o "Buenas"(a George).

```
{
      "Action": "EditData",
      "Target": "mistyspring.dynamicdialogues/Greetings",
      "Entries": {
        "Alex": {
          "Evelyn": "Hola",
          "George": "Buenas"
        }
      }
    }
```
