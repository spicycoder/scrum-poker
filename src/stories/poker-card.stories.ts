import type { Meta, StoryObj } from '@storybook/angular';
import { PokerCardComponent } from '../app/components/poker-card/poker-card.component';

const meta: Meta<PokerCardComponent> = {
  title: 'Example/PokerCard',
  component: PokerCardComponent,
  tags: ['autodocs'],
};

export default meta;
type Story = StoryObj<PokerCardComponent>;

/**
 * For displaying players
 */
export const Player: Story = {
  args: {
    card: {
      display: 'John Doe',
      selected: true,
      value: null,
    },
  },
};

/**
 * For displaying players, with respective point they voted
 */
export const PlayerWithValue: Story = {
  args: {
    card: {
      display: 'John Doe',
      value: '3',
      selected: true,
    },
  },
};

/**
 * For displaying players, with respective point they voted
 */
export const PlayerWithHiddenValue: Story = {
  args: {
    card: {
      display: 'John Doe',
      value: '3',
      selected: true,
    },
    show: false
  },
};

/**
 * For displaying selected point
 */
export const PointSelected: Story = {
  args: {
    card: {
      display: '5',
      selected: true,
      value: null,
    },
  },
};

/**
 * For displaying all points (to be selected)
 */
export const PointUnselected: Story = {
  args: {
    card: {
      display: '5',
      selected: false,
      value: null,
    },
  },
};

/**
 * To display point against how many folks selected it
 */
export const PointWithCount: Story = {
  args: {
    card: {
      display: '5',
      value: '3',
      selected: true,
    },
  },
};
