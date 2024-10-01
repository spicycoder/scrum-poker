import type { Meta, StoryObj } from '@storybook/angular';
import { PointsComponent } from '../app/components/points/points.component';

const meta: Meta<PointsComponent> = {
  title: 'Example/Points',
  component: PointsComponent,
  tags: ['autodocs'],
};

export default meta;
type Story = StoryObj<PointsComponent>;

/**
 * For displaying players
 */
export const PointsAllVoted: Story = {
  args: {
    points: [
      {
        display: 'Peter Parker',
        selected: true,
        value: '5',
      },
      {
        display: 'Tony Stark',
        selected: true,
        value: '5',
      },
      {
        display: 'Bruce Wayne',
        selected: true,
        value: '3',
      },
      {
        display: 'Steve Rogers',
        selected: true,
        value: '1',
      },
      {
        display: 'Bruce Banner',
        selected: true,
        value: '8',
      },
      {
        display: 'Natasha Romanoff',
        selected: true,
        value: '5',
      },
      {
        display: 'Stephen Strange',
        selected: true,
        value: '13',
      },
      {
        display: 'Barry Allen',
        selected: true,
        value: '13',
      },
      {
        display: 'Clark Kent',
        selected: true,
        value: '13',
      },
      {
        display: 'Diana Prince',
        selected: true,
        value: '5',
      },
    ]
  },
};

/**
 * For displaying players
 */
export const PointsNotEveryoneVoted: Story = {
  args: {
    points: [
      {
        display: 'Peter Parker',
        selected: true,
        value: '5',
      },
      {
        display: 'Tony Stark',
        selected: true,
        value: '5',
      },
      {
        display: 'Bruce Wayne',
        selected: true,
        value: null,
      },
      {
        display: 'Steve Rogers',
        selected: true,
        value: '1',
      },
      {
        display: 'Bruce Banner',
        selected: true,
        value: '8',
      },
      {
        display: 'Natasha Romanoff',
        selected: true,
        value: '5',
      },
      {
        display: 'Stephen Strange',
        selected: true,
        value: '13',
      },
      {
        display: 'Barry Allen',
        selected: true,
        value: null,
      },
      {
        display: 'Clark Kent',
        selected: true,
        value: '13',
      },
      {
        display: 'Diana Prince',
        selected: true,
        value: '5',
      },
    ]
  },
};
