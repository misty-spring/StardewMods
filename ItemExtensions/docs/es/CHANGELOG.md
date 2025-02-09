# CAMBIOS

## 1.12.2
- Se corrigió eror donde las explosiones no generaban escaleras
- Se corrigió eror donde las explosiones no generaban items extra
- Se corrigió NRE al llamar who.HasBuff en TryExtraDrops

## 1.12.1
- Se corrigió error al romper recursos grandes relacionado a string nula

## 1.12.0
- Recursos personalizados ahora generan escaleras (10%, configurable a través de archivo de configuración)
- Se arregló comportamiento de semillas mixtas
- Compatibilidad con bendiciones de minería y mastery
- Se puede personalizar la textura al beber

## 1.11.2
- Se actualizó el idioma Chino, por 2228091075

## 1.11.1
- Se agregó el idioma Coreano, por yuuyeeyii
- Herramientas intercambiadas son re-encantadas
- Actualización de métodos de API 

## 1.11.0
- Se actualizó para la versión 1.6.9 del juego
- Si un item encantado es intercambiado, el juego dará una esquirla prismática como compensación.
- Se corrigió error sobre recursos grandes que no se generaban en la caverna calavera de Qi.
- Se corrigió error donde monstruos de recursos se creaban en las coordenadas equivocadas.
- Nuevo método de API

## 1.10.0
- Se agregó `ChanceDropOnFront` para items de trenes.

## 1.9.1
- Se arregló error al generar monstruos de recurso

## 1.9.0
- Se agregó AvoidItemIds (`List<string>`) en ISpawnItemData: ahora puedes evitar ciertas IDs al generar ítems
- Se corrigió error en condiciones/cantidad de generación
- Items de tren: El eje X ahora varía un poco

## 1.8.1
- Se devolvieron algunos cambios de API

## 1.8.0
- Se agregaron recursos vanilla al método de API `IsClump`.
- Se agregaron más condiciones para los items de tesoros de pesca.
- Se cambió el diccionario `/Treasure` para tener un valor `TreasureData`.
- Pequeña refactorización

## 1.7.0
- Se removieron semillas incorrectas de `CropPatches.GetVanillaCropsForSeason`
- Se corrigió error donde el Tractor Mod no plantaba semillas mixtas personalizadas
- Ahora se puede personalizar el texto al comer (por aceynk)

## 1.6.1
- Se corrigió error en recompensas de pesca
- Se arregló error donde las semillas no podían ser plantadas
- Se actualizó traducción al francés (por Caranud)
- Se actualizó traducción al chino (por Awassakura)

## 1.6.0
- Corrección de error para items de tren
- Nuevos OnBehaviors: OnAttached, OnDetached
- Soporte para tesoros de pesca personalizados
- Cambios en API 

## 1.5.1
- Nuevo método de API
- Corrección de error para semillas mixtas (tentativo) 

## 1.5.0
- Se corrigió error donde algunos cultivos cambiaban aleatoriamente
- Se corrigió error donde algunas armas realizaban 0 daño
- Se agregaron ítems personalizados al tren
- Ahora puede generar nodos en volcán y montaña
- Se agregó opción "frenzy" para generar nodos, similar a niveles de hongo
- Puede generar en las minas: árboles, árboles frutales, cultivos gigantes
- Puede forzar que los ítems siempre tengan una calidad específica
- Puede activar o desactivar componentes específicos
- Puede agregar items que no sean peces a la piscina.
- Los nodos tienen la etiqueta "placeable" (colocable) por defecto.

## 1.4.3
- Se corrigió error en compatibilidad con Tractor Mod (final)

## 1.4.2
- Corrección de errores

## 1.4.1
- Se corrigió un eror para clumps en la mina

## 1.4.0
- Corrección de error para acciones de menú y generación de ítems en multijugador
- Se cambió cómo se guardan las acciones de menú

## 1.3.0
- Compatibilidad con Tractor Mod.
- Ahora puede definir días máximos para un recurso a través de los CustomFields en Data/Locations
- Ahora se pueden agregar ítems a la batea

## 1.2.2
- Corrección de errores

## 1.2.1
- Corrección de errores

## 1.2.0
- Se implementó generación en minas para recursos

## 1.1.0
- Recursos pueden generarse en la mina ahora
- Cambios en API
- Mayor compatibilidad para semillas mixtas vainilla

## 1.0.0
Versión inicial.
