# Pokédex – Blazor WebAssembly

Uma Pokédex feita em Blazor WebAssembly com busca, listagem, detalhes completos, CRUD de favoritos, filtros por tipo, e relação de fraquezas/resistências/imunidades.

## Requisitos do sistema
- .NET SDK 8.0+
- Node.js (opcional, para métricas Lighthouse)
- Navegador moderno (Chrome, Edge, Firefox, Safari)

## Instalação e execução
1. Restaurar dependências:
   - `dotnet restore`
2. Rodar em modo desenvolvimento (Dev Server):
   - `dotnet run`
   - Acesse `http://localhost:5000`
3. (Opcional) Métricas de performance e acessibilidade:
   - `npm install`
   - `npm run audit:perf`

## Funcionalidades
- Busca por nome ou ID com sugestões
- Listagem com nome, ID e sprite
- Página de detalhes:
  - Tipos com badges e capitalização automática
  - Habilidades com descrição
  - Estatísticas em barras
  - Movimentos (agrupados por método e tipo) com indicadores de poder/precisão/PP
  - Relações de tipo: fraquezas (2×), resistências (0,5×), imunidades (0× com ícone)
- Favoritos (CRUD completo):
  - Adicionar/remover
  - Editar notas
  - Editar tags
  - Persistência local via `localStorage`
- Filtros por tipo com opção “Normal” para typeless

## Arquitetura e tecnologias
- Blazor WebAssembly (`Microsoft.AspNetCore.Components.WebAssembly`)
- Dev Server (`Microsoft.AspNetCore.Components.WebAssembly.DevServer`)
- PokeAPI para dados de Pokémon
- CSS simples e utilitários JS em `wwwroot/index.html`

## Persistência local
- Serviço `LocalStorageService` (`Services/LocalStorageService.cs`) abstrai operações de `localStorage` via `IJSRuntime`.
- Chaves principais:
  - `favorites`: lista de favoritos com `PokemonId`, `Note`, `Tags`

## Critérios de aceitação (como validar)
- Busca: digite o nome ou ID na página de gerações/índice; Enter abre detalhes
- Favoritos: no detalhe, clique em “Adicionar aos favoritos”; remova para alternar estado
- Persistência: abra a aba de favoritos; alterações de notas/tags permanecem após recarregar

## Testes automatizados
- `dotnet test`
- Cobertura principal:
  - Normalização de tipos e comparação de filtros (`Pokedex.Tests/TypeUtilsTests.cs`, `Pokedex.Tests/TypeRelationsTests.cs`)
  - Validação de ranges de gerações (`Pokedex.Tests/GenerationUtilsTests.cs`)

## Guia de uso
- Navegue pelas gerações e use a busca para localizar Pokémon
- Abra o detalhe para ver tipos, habilidades, estatísticas, movimentos e relações de tipo
- Marque como favorito, adicione notas e tags; veja e gerencie na aba Favoritos
- Filtre por tipo nas listas e favoritos (inclui “Normal”)

