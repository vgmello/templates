# Page snapshot

```yaml
- link "Skip to main content":
  - /url: "#main-content"
- banner:
  - navigation:
    - link "Billing Service":
      - /url: /
    - link "Cashiers":
      - /url: /cashiers
- main:
  - button:
    - img
  - heading "Create New Cashier" [level=1]
  - paragraph: Add a new cashier to handle payments for your business.
  - heading "Basic Information" [level=3]
  - text: Name *
  - textbox "Name *"
  - text: Email *
  - textbox "Email *"
  - heading "Supported Currencies" [level=3]
  - text: USD
  - button:
    - img
  - textbox "Add currency (e.g., USD)"
  - button [disabled]:
    - img
  - paragraph: "Quick add popular currencies:"
  - button "EUR"
  - button "GBP"
  - button "CAD"
  - button "AUD"
  - button "JPY"
  - button "CNY"
  - button "INR"
  - button "BRL"
  - button "Create Cashier"
  - button "Cancel"
```