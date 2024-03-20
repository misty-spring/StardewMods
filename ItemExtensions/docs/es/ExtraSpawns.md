# Extra Spawns

# Contenidos

* [Formato](#formato)
* [Ejemplos](#ejemplos)

--------------------

## Formato

ExtraSpawn tiene estos campos:

| nombre      | tipo                  | obligatorio | descripción                            |
|-------------|-----------------------|-------------|----------------------------------------|
| ItemId      | `string`              | Si          | Id calificada del objeto.              |
| Chance      | `double`              | No          | Probabilidad: e.j, 0.5 para 50%.       |
| Condition   | `string`              | No          | Consulta al estado del juego.\*        |
| AvoidRepeat | `bool`                | No          | Avanzado, se usa con `ISpawnItemData`. |
| Filter      | `ItemQuerySearchMode` | No          | Avanzado, se usa con `ISpawnItemData`. |

\* = Las consultas al estado del juego fueron agregadas en 1.6. Para ver las que agrega el juego base, [ve a la wiki](https://stardewvalleywiki.com/Modding:Game_state_queries).

Ademas de esto, **también** tienen todos los [campos de generación de item](https://stardewvalleywiki.com/Modding:Item_queries#Item_spawn_fields).


## Ejemplos


```jsonc
[
  {
    "ItemId": "330",              //arcilla
    "Chance": 1                   //100% de aparecer
  },
  {
    "ItemId": "(O)168",           //basura
    "Chance": 0.3,                //30% de aparecer
    "Condition": "SEASON summer"  //sólo en verano
  }
]
```