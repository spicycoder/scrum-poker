import type { Meta, StoryObj } from '@storybook/react';
import GooeyNav, { type GooeyNavItem } from './GooeyNav';

const fibonacciItems: GooeyNavItem[] = [
  '0', '1', '2', '3', '5', '8', '13', '21', '?',
].map(label => ({ label }));

const tshirtItems: GooeyNavItem[] = [
  'XS', 'S', 'M', 'L', 'XL', 'XXL', '?',
].map(label => ({ label }));

const meta: Meta<typeof GooeyNav> = {
  title: 'Components/GooeyNav',
  component: GooeyNav,
  tags: ['autodocs'],
};

export default meta;
type Story = StoryObj<typeof GooeyNav>;

export const Fibonacci: Story = {
  args: {
    items: fibonacciItems,
    initialActiveIndex: -1,
  },
};

export const TShirtSizes: Story = {
  args: {
    items: tshirtItems,
    initialActiveIndex: -1,
  },
};

const active: number = 3;

export const ThirdItemActive: Story = {
  args: {
    items: [{
      "label": "🍋"
    }, {
      "label": "🍊"
    }, {
      "label": "🥭"
    }, {
      "label": "🍍"
    }, {
      "label": "🍎"
    }, {
      "label": "🍈"
    }],
    initialActiveIndex: active - 1,
  },
};
