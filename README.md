# function-triagem-mensagem
Azure Function responsável por ler a mensagem enviada pelo cliente e decidir qual passo executar com base nela.

## Trigger
A função será acionada via HTTP Request pelo Webhook da Twilio.

## Saída
Salvar ou buscar registros conforme parametrização das mensagens vindas do Whatsapp.
