SELECT DISTINCT c.nombre
FROM clientes c
JOIN inscripciones i ON c.id_cliente = i.id_cliente
JOIN productos p ON i.id_producto = p.id_producto
JOIN sucursales_visitadas s ON c.id_cliente = s.id_cliente
WHERE p.id_sucursal = s.id_sucursal;
