#!/usr/bin/env bash
# Fires 20 requests and prints the status code of each one.
for i in $(seq -w 1 20); do
  code=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:5081/api/orders)
  echo "#$i  $code"
done
