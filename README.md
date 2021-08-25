# Moneris
Login to Moneris
 * [Merchant Resource Center (MRC or MPG)](https://www3.moneris.com/mpg/index.php)
 * [Merchant Direct (MD)](https://www1.moneris.com/merchantdirect)

I cannot believe that there is no API for Moneris transactions. But today (2021-08-05), it is not so hard to make one GET and POST requests to simulate user access into both MRC and MD. Then, it takes one more POST to download transactions or anything else.

The Moneris API is poor but their “home made” security is a disaster. I cannot understand why their services are so expensive.

## Merchant Resource Center
If you need to simulate login to MRC:
* Send GET request to [MRC login form](https://www3.moneris.com/mpg/index.php)
* Send POST request to [MRC login form](https://www3.moneris.com/mpg/index.php) and use cookies and dynamic input field names of login form from the previous GET + credentials. 
  * The input names in the login form are dynamic and required in this POST request but the order of these inputs is fixed.
* You are logged in... Use cookies from response for further activities.

## Merchant Direct
If you need to simulate login to MD:
* Send GET request to [MD login form]((https://www1.moneris.com/cgi-bin/rbaccess/rbunxcgi?F6=1&F7=L8&F21=PB&F22=L8&REQUEST=ClientSignin&LANGUAGE=ENGLISH)
  * Note: Use URL from my link where https://www1.moneris.com/merchantdirect is redirected
* Send POST request to [MD login form](https://www1.moneris.com/cgi-bin/rbaccess/rbunxcgi?F6=1&F7=L8&F21=PB&F22=L8&REQUEST=ClientSignin&LANGUAGE=ENGLISH) and use hidden input fields from the response to your previous GET request (esp. `<INPUT NAME="SST" TYPE="HIDDEN" VALUE="this_is_verified_value">` is needed) + your credentials (`<input NAME="USERID">` and `<input NAME="PASSWORD">`). 
* You are logged in... Use cookies from response for further activities.

Let me know if you need code example.
