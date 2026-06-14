import { addons } from 'storybook/internal/manager-api';
import { create } from 'storybook/theming/create';

addons.setConfig({
  theme: create({
    base: 'dark',
    brandTitle: '♠ Scrum Poker',
    brandUrl: '/',
  }),
});
