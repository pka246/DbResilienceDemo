# Fires 20 requests and prints the status code of each one (PowerShell 7+).
1..20 | ForEach-Object {
    $r = Invoke-WebRequest http://localhost:5081/api/orders -SkipHttpErrorCheck
    "#{0:00}  {1}" -f $_, $r.StatusCode
}
